using ClosedXML.Excel;
using CuaHang.Api.Common;
using CuaHang.Api.Dtos;
using CuaHang.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace CuaHang.Api.Controllers;

[ApiController]
[Route("api/bao-cao")]
public sealed class BaoCaoController(
    BaoCaoService service,
    IMemoryCache cache) : ControllerBase
{
    [HttpGet("doanh-so-san-pham-theo-thang")]
    [ProducesResponseType(
        typeof(PhanHoi<PaginateResponse<BaoCaoSanPhamThangResponse>>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PhanHoi<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DoanhSoTheoSanPhamTheoThang(
        [FromQuery] DateTime? tuNgay,
        [FromQuery] DateTime? denNgay,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (!TryChuanHoaKhoang(tuNgay, denNgay, page, pageSize,
                out var batDauUtc, out var ketThucUtc, out var loi))
            return loi!;

        var ketQua = await LayBaoCaoAsync(
            batDauUtc, ketThucUtc, page, pageSize, ct);
        return Ok(PhanHoi.Ok(ketQua));
    }

    [HttpGet("doanh-so-san-pham-theo-thang/excel")]
    [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PhanHoi<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> XuatDoanhSoExcel(
        [FromQuery] DateTime? tuNgay,
        [FromQuery] DateTime? denNgay,
        CancellationToken ct = default)
    {
        if (!TryChuanHoaKhoang(tuNgay, denNgay, 1, 200,
                out var batDauUtc, out var ketThucUtc, out var loi))
            return loi!;

        const int pageSize = 200;
        const int maximumRows = 10_000;
        var ketQua = await LayBaoCaoAsync(batDauUtc, ketThucUtc, 1, pageSize, ct);
        if (ketQua.TongSo > maximumRows)
            return BadRequest(PhanHoi.Loi(
                $"Báo cáo vượt giới hạn xuất Excel {maximumRows:N0} dòng"));

        var danhSach = new List<BaoCaoSanPhamThangResponse>(ketQua.DanhSach);
        var totalPages = (ketQua.TongSo + pageSize - 1) / pageSize;
        for (var page = 2; page <= totalPages; page++)
        {
            var result = await LayBaoCaoAsync(
                batDauUtc, ketThucUtc, page, pageSize, ct);
            danhSach.AddRange(result.DanhSach);
        }

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Doanh so");
        worksheet.Cell(1, 1).Value = "Mã sản phẩm";
        worksheet.Cell(1, 2).Value = "Tên sản phẩm";
        worksheet.Cell(1, 3).Value = "Tháng";
        worksheet.Cell(1, 4).Value = "Doanh số";
        worksheet.Cell(1, 5).Value = "Số đơn hàng";
        worksheet.Row(1).Style.Font.Bold = true;

        for (var index = 0; index < danhSach.Count; index++)
        {
            var item = danhSach[index];
            var row = index + 2;
            worksheet.Cell(row, 1).Value = item.SanPhamId;
            worksheet.Cell(row, 2).Value = item.Ten;
            if (item.Thang.HasValue)
                worksheet.Cell(row, 3).Value = item.Thang.Value;
            worksheet.Cell(row, 3).Style.DateFormat.Format = "yyyy-mm";
            worksheet.Cell(row, 4).Value = item.DoanhSo;
            worksheet.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(row, 5).Value = item.SoDonHang;
        }

        worksheet.Columns().AdjustToContents();
        using var output = new MemoryStream();
        workbook.SaveAs(output);
        return File(
            output.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"doanh-so-{batDauUtc:yyyyMMdd}-{ketThucUtc:yyyyMMdd}.xlsx");
    }

    private async Task<PaginateResponse<BaoCaoSanPhamThangResponse>> LayBaoCaoAsync(
        DateTime tuNgay,
        DateTime denNgay,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        var cacheKey = $"bao-cao-san-pham:{tuNgay.Ticks}:{denNgay.Ticks}:{page}:{pageSize}";
        var result = await cache.GetOrCreateAsync(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            return service.DoanhSoTheoSanPhamTheoThangAsync(
                tuNgay, denNgay, page, pageSize, ct);
        });
        return result ?? throw new InvalidOperationException("Không tạo được dữ liệu báo cáo");
    }

    private static bool TryChuanHoaKhoang(
        DateTime? tuNgay,
        DateTime? denNgay,
        int page,
        int pageSize,
        out DateTime batDauUtc,
        out DateTime ketThucUtc,
        out IActionResult? loi)
    {
        batDauUtc = default;
        ketThucUtc = default;
        var errors = new List<LoiTruong>();
        if (tuNgay is null)
            errors.Add(new("tuNgay", "tuNgay là bắt buộc"));
        if (denNgay is null)
            errors.Add(new("denNgay", "denNgay là bắt buộc"));
        if (page < 1 || (long)(page - 1) * pageSize > int.MaxValue)
            errors.Add(new("page", "page không hợp lệ"));
        if (pageSize is < 1 or > 200)
            errors.Add(new("pageSize", "pageSize phải từ 1 đến 200"));
        if (errors.Count > 0)
        {
            loi = new BadRequestObjectResult(PhanHoi.Loi("Dữ liệu không hợp lệ", errors));
            return false;
        }

        batDauUtc = ChuanHoaUtc(tuNgay!.Value);
        ketThucUtc = ChuanHoaUtc(denNgay!.Value);
        if (batDauUtc >= ketThucUtc)
            errors.Add(new("denNgay", "denNgay phải sau tuNgay"));
        else if (batDauUtc <= DateTime.MaxValue.AddMonths(-12)
            && ketThucUtc > batDauUtc.AddMonths(12))
            errors.Add(new("denNgay", "Khoảng báo cáo không được vượt quá 12 tháng"));

        loi = errors.Count == 0
            ? null
            : new BadRequestObjectResult(PhanHoi.Loi("Dữ liệu không hợp lệ", errors));
        return errors.Count == 0;
    }

    private static DateTime ChuanHoaUtc(DateTime value)
        => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
}
