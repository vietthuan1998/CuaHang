using CuaHang.Api.Common;
using CuaHang.Api.Dtos;
using CuaHang.Api.Entities;
using CuaHang.Api.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace CuaHang.Api.Controllers;

[ApiController]
[Route("api/don-hang")]
public sealed class DonHangController(DonHangService service, HoaDonService hoaDonService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(
        typeof(PhanHoi<PaginateResponse<DonHangDanhSachResponse>>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PhanHoi<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TimDanhSach(
        [FromQuery] string? trangThai = null,
        [FromQuery] DateTime? tuNgay = null,
        [FromQuery] DateTime? denNgay = null,
        [FromQuery] int? khachHangId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var loi = new List<LoiTruong>();
        if (!ModelState.IsValid)
            loi.Add(new("query", "Tham số truy vấn không hợp lệ"));
        if (page < 1 || (long)(page - 1) * pageSize > int.MaxValue)
            loi.Add(new("page", "page không hợp lệ"));
        if (pageSize is < 1 or > 200)
            loi.Add(new("pageSize", "pageSize phải từ 1 đến 200"));
        if (khachHangId is <= 0)
            loi.Add(new("khachHangId", "khachHangId phải lớn hơn 0"));

        TrangThaiDon? trangThaiDon = null;
        if (!string.IsNullOrWhiteSpace(trangThai))
        {
            if (!Enum.TryParse<TrangThaiDon>(trangThai, false, out var parsed)
                || !Enum.IsDefined(parsed))
                loi.Add(new("trangThai", "Trạng thái không hợp lệ"));
            else
                trangThaiDon = parsed;
        }

        if (tuNgay.HasValue && denNgay.HasValue
            && ChuanHoaUtc(tuNgay.Value) >= ChuanHoaUtc(denNgay.Value))
            loi.Add(new("denNgay", "denNgay phải sau tuNgay"));
        if (loi.Count > 0)
            return BadRequest(PhanHoi.Loi("Dữ liệu không hợp lệ", loi));

        var ketQua = await service.TimAsync(
            trangThaiDon,
            tuNgay.HasValue ? ChuanHoaUtc(tuNgay.Value) : null,
            denNgay.HasValue ? ChuanHoaUtc(denNgay.Value) : null,
            khachHangId,
            page,
            pageSize,
            User.Identity?.Name,
            User.IsInRole("QuanTri"),
            ct);
        return Ok(PhanHoi.Ok(ketQua));
    }

    [HttpPost]
    [ProducesResponseType(typeof(PhanHoi<DonHangResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(PhanHoi<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PhanHoi<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DatHang(
        [FromBody] DatHangRequest? request,
        CancellationToken ct)
    {
        var idempotencyKey = Request.Headers["Idempotency-Key"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(idempotencyKey) || idempotencyKey.Length > 100)
            return BadRequest(PhanHoi.Loi("Idempotency-Key là bắt buộc và tối đa 100 ký tự", [
                new LoiTruong("Idempotency-Key", "Cần gửi khóa idempotency dài từ 1 đến 100 ký tự")
            ]));
        if (!ModelState.IsValid)
        {
            var loiModel = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .Select(x => new LoiTruong(x.Key, "Giá trị không hợp lệ"))
                .ToList();
            return BadRequest(PhanHoi.Loi("Dữ liệu không hợp lệ", loiModel));
        }
        if (request is null)
            return BadRequest(PhanHoi.Loi("Json không hợp lệ"));

        var loi = new List<LoiTruong>();
        if (request.KhachHangId is null or <= 0)
            loi.Add(new("khachHangId", "khachHangId phải lớn hơn 0"));
        if (request.SanPhamId is null or <= 0)
            loi.Add(new("sanPhamId", "sanPhamId phải lớn hơn 0"));
        if (request.SoLuong is null or <= 0)
            loi.Add(new("soLuong", "soLuong phải lớn hơn 0"));
        if (loi.Count > 0)
            return BadRequest(PhanHoi.Loi("Dữ liệu không hợp lệ", loi));

        var donHang = await service.DatHangAsync(
            request, idempotencyKey, User.Identity?.Name ?? string.Empty, ct);
        return CreatedAtAction(
            nameof(LayTheoMa),
            new { id = donHang.Id },
            PhanHoi.Ok(donHang));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PhanHoi<DonHangResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PhanHoi<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LayTheoMa(int id, CancellationToken ct)
    {
        if (!User.IsInRole("QuanTri")
            && (User.Identity?.Name is not string tenDangNhap
                || !await service.LaNguoiTaoAsync(id, tenDangNhap, ct)))
            return Forbid();

        var donHang = await service.LayTheoMaAsync(id, ct);
        return donHang is null
            ? NotFound(PhanHoi.Loi("Không tìm thấy đơn hàng"))
            : Ok(PhanHoi.Ok(donHang));
    }

    [HttpPost("{id:int}/huy")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(PhanHoi<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(PhanHoi<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Huy(int id, CancellationToken ct)
    {
        if (!User.IsInRole("QuanTri")
            && (User.Identity?.Name is not string tenDangNhap
                || !await service.LaNguoiTaoAsync(id, tenDangNhap, ct)))
            return Forbid();

        await service.HuyAsync(id, ct);
        return NoContent();
    }

    [HttpGet("{id:int}/hoa-don")]
    [Produces("application/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PhanHoi<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LayHoaDon(int id, CancellationToken ct)
    {
        if (!User.IsInRole("QuanTri")
            && (User.Identity?.Name is not string tenDangNhap
                || !await service.LaNguoiTaoAsync(id, tenDangNhap, ct)))
            return Forbid();

        var pdf = await hoaDonService.TaoPdfAsync(id, ct);
        return pdf is null
            ? NotFound(PhanHoi.Loi("Không tìm thấy đơn hàng"))
            : File(pdf, "application/pdf", $"hoa-don-{id}.pdf");
    }

    private static DateTime ChuanHoaUtc(DateTime value)
        => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
}
