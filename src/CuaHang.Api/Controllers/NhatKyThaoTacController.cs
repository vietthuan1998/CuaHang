using CuaHang.Api.Common;
using CuaHang.Api.Dtos;
using CuaHang.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CuaHang.Api.Controllers;

[ApiController]
[Route("api/nhat-ky-thao-tac")]
[Authorize(Roles = "QuanTri")]
public class NhatKyThaoTacController : ControllerBase
{
    private static readonly string[] TenBangChoPhep = { "SanPhams", "DonHangs" };

    private readonly NhatKyService _service;

    public NhatKyThaoTacController(NhatKyService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(PhanHoi<PaginateResponse<NhatKyResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PhanHoi<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Tim(
        [FromQuery] string? tenBang,
        [FromQuery] string? banGhiId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var loi = new List<LoiTruong>();
        if (pageSize is < 1 or > 200)
            loi.Add(new("pageSize", "pageSize phải từ 1 đến 200"));
        if (page < 1 || (long)(page - 1) * pageSize > int.MaxValue)
            loi.Add(new("page", "page không hợp lệ"));
        if (!string.IsNullOrEmpty(tenBang) && !TenBangChoPhep.Contains(tenBang))
            loi.Add(new("tenBang", "Tên bảng không được hỗ trợ"));
        if (loi.Count > 0)
            return BadRequest(PhanHoi.Loi("Dữ liệu không hợp lệ", loi));

        var ketQua = await _service.TimAsync(tenBang, banGhiId, page, pageSize, ct);
        return Ok(PhanHoi.Ok(ketQua));
    }
}
