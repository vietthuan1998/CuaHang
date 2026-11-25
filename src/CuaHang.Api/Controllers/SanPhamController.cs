using CuaHang.Api.Common;
using CuaHang.Api.Dtos;
using CuaHang.Api.Services;
using CuaHang.Api.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CuaHang.Api.Controllers
{
    [Route("api/san-pham")]
    [ApiController]
    public class SanPhamController : ControllerBase
    {
        private readonly SanPhamService _service;
        private readonly SanPhamAnhService _anhService;

        public SanPhamController(SanPhamService service, SanPhamAnhService anhService)
        {
            _service = service;
            _anhService = anhService;
        }

            [HttpGet]
            [ProducesResponseType(typeof(PhanHoi<PaginateResponse<SanPhamResponse>>), StatusCodes.Status200OK)]
            [ProducesResponseType(typeof(PhanHoi<object>), StatusCodes.Status400BadRequest)]
            public async Task<IActionResult> TimDanhSach(
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 20,
                [FromQuery] string? tuKhoa = null,
                [FromQuery] int? danhMucId = null,
                [FromQuery] string? sapXep = null,
                CancellationToken ct = default)
            {
                var loi = new List<LoiTruong>();
                if (page < 1 || (long)(page - 1) * pageSize > int.MaxValue)
                    loi.Add(new("page", "page không hợp lệ"));
                if (pageSize is < 1 or > 200)
                    loi.Add(new("pageSize", "pageSize phải từ 1 đến 200"));
                if (danhMucId is <= 0)
                    loi.Add(new("danhMucId", "danhMucId phải lớn hơn 0"));
                var sapXepChuan = string.IsNullOrWhiteSpace(sapXep) ? "ten" : sapXep;
                if (sapXepChuan is not ("ten" or "donGia" or "-ten" or "-donGia"))
                    loi.Add(new("sapXep", "Giá trị sapXep không được hỗ trợ"));
                if (loi.Count > 0)
                    return BadRequest(PhanHoi.Loi("Dữ liệu không hợp lệ", loi));

                var ketQua = await _service.TimAsync(
                    tuKhoa, danhMucId, sapXepChuan, page, pageSize, ct);
                return Ok(PhanHoi.Ok(ketQua));
            }

            [HttpGet("ton-thap")]
            [ProducesResponseType(typeof(PhanHoi<IReadOnlyList<SanPhamResponse>>), StatusCodes.Status200OK)]
            [ProducesResponseType(typeof(PhanHoi<object>), StatusCodes.Status400BadRequest)]
            public async Task<IActionResult> TimTonThap(
                [FromQuery] int? nguong,
                CancellationToken ct = default)
            {
                if (nguong is null or < 0)
                    return BadRequest(PhanHoi.Loi("Dữ liệu không hợp lệ", [
                        new LoiTruong("nguong", "nguong phải lớn hơn hoặc bằng 0")
                    ]));

                var ketQua = await _service.TimTonThapAsync(nguong.Value, ct);
                return Ok(PhanHoi.Ok(ketQua));
            }

            [HttpPut("{id:int}/ton-kho")]
            [Authorize(Policy = "DuocSuaSanPham")]
            [ProducesResponseType(typeof(PhanHoi<SanPhamResponse>), StatusCodes.Status200OK)]
            [ProducesResponseType(typeof(PhanHoi<object>), StatusCodes.Status400BadRequest)]
            [ProducesResponseType(typeof(PhanHoi<object>), StatusCodes.Status404NotFound)]
            [ProducesResponseType(typeof(PhanHoi<object>), StatusCodes.Status409Conflict)]
            public async Task<IActionResult> CapNhatTonKho(
                int id,
                [FromBody] CapNhatTonKhoRequest? request,
                CancellationToken ct)
            {
                if (!ModelState.IsValid)
                    return BadRequest(PhanHoi.Loi("Dữ liệu không hợp lệ"));
                if (request?.TonKho is null or < 0)
                    return BadRequest(PhanHoi.Loi("Tồn kho phải lớn hơn hoặc bằng 0", [
                        new LoiTruong("tonKho", "Tồn kho phải lớn hơn hoặc bằng 0")
                    ]));
                if (string.IsNullOrWhiteSpace(request.RowVersion))
                    return BadRequest(PhanHoi.Loi("Thiếu rowVersion", [
                        new LoiTruong("rowVersion", "rowVersion là bắt buộc")
                    ]));

                byte[] rowVersion;
                try
                {
                    rowVersion = Convert.FromBase64String(request.RowVersion);
                }
                catch (FormatException)
                {
                    return BadRequest(PhanHoi.Loi("rowVersion không hợp lệ", [
                        new LoiTruong("rowVersion", "rowVersion phải là chuỗi Base64 hợp lệ")
                    ]));
                }
                if (rowVersion.Length != 8)
                    return BadRequest(PhanHoi.Loi("rowVersion không hợp lệ", [
                        new LoiTruong("rowVersion", "rowVersion phải có đúng 8 byte")
                    ]));

                var ketQua = await _service.CapNhatTonKhoAsync(id, request.TonKho.Value, rowVersion, ct);
                return Ok(PhanHoi.Ok(ketQua));
            }

            [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(PhanHoi<SanPhamResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PhanHoi<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> LayTheoMa([FromRoute] int id, CancellationToken ct = default)
        {
            var result = await _service.LayTheoMaAsync(id, ct);
            return result is null ?
                NotFound(PhanHoi.Loi("Không có dữ liệu")) :
                Ok(PhanHoi.Ok(result));
        }
        [HttpPost]
        [Authorize(Policy = "DuocSuaSanPham")]
        [ProducesResponseType(typeof(PhanHoi<SanPhamResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(PhanHoi<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(PhanHoi<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> TaoSanPham([FromBody] SanPhamRequest? sp, CancellationToken ct = default)
        {
            if (!ModelState.IsValid)
            {
                var loiModel = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .Select(x => new LoiTruong(x.Key, "Giá trị không hợp lệ"))
                    .ToList();
                return BadRequest(PhanHoi.Loi("Dữ liệu không hợp lệ", loiModel));
            }

            if (sp == null)
                return BadRequest(PhanHoi.Loi("Json không hợp lệ"));
            SanPhamRequest newSp = sp with
            {
                Ten = sp.Ten?.Trim(),
                GhiChu = sp.GhiChu?.Trim()
            };

            var loi = SanPhamValidator.Kiem(newSp);

            if (loi.Count > 0)
                return BadRequest(PhanHoi.Loi("Dữ liệu không phù hợp:", loi));

            if (newSp.DanhMucId is not int danhMucId || !await _service.DanhMucTonTai(danhMucId, ct))
                return BadRequest(PhanHoi.Loi("Danh mục không tồn tại"));
            var result = await _service.TaoSanPhamAsync(newSp, ct);
            return CreatedAtAction(nameof(LayTheoMa), new { id = result.Id }, PhanHoi.Ok(result));

        }
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "DuocSuaSanPham")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(PhanHoi<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> XoaSanPham([FromRoute] int id, CancellationToken ct = default)
        {
            var kiemTra = await _service.XoaSanPham(id, ct);
            return !kiemTra ?
                NotFound(PhanHoi.Loi("Không tìm thấy sản phẩm")) :
                NoContent();
        }

        [HttpPost("{id:int}/anh")]
        [Authorize(Policy = "DuocSuaSanPham")]
        [RequestSizeLimit(6 * 1024 * 1024)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(PhanHoi<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(PhanHoi<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> TaiAnh(int id, IFormFile? tep, CancellationToken ct)
        {
            if (tep is null)
                return BadRequest(PhanHoi.Loi("Vui lòng gửi tệp ảnh trong trường 'tep'"));

            await _anhService.LuuAnhAsync(id, tep, ct);
            return NoContent();
        }

        [HttpGet("{id:int}/anh")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PhanHoi<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> LayAnh(int id, CancellationToken ct)
        {
            var (stream, contentType) = await _anhService.MoAnhAsync(id, ct);
            return File(stream, contentType);
        }
    }
}
