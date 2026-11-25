using CuaHang.Api.Common;
using CuaHang.Api.Dtos;
using CuaHang.Api.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CuaHang.Api.Controllers;

[ApiController]
[Route("api/xac-thuc")]
[AllowAnonymous]
public sealed class XacThucController(XacThucService service) : ControllerBase
{
    [HttpPost("dang-nhap")]
    [EnableRateLimiting("dang-nhap")]
    [ProducesResponseType(typeof(PhanHoi<DangNhapResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PhanHoi<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PhanHoi<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DangNhap(
        [FromBody] DangNhapRequest? request,
        CancellationToken ct)
    {
        if (request is null || !ModelState.IsValid)
            return BadRequest(PhanHoi.Loi("Thông tin đăng nhập không hợp lệ"));

        var ketQua = await service.DangNhapAsync(request, ct);
        return ketQua is null
            ? Unauthorized(PhanHoi.Loi("Tên đăng nhập hoặc mật khẩu không đúng"))
            : Ok(PhanHoi.Ok(ketQua));
    }

    [HttpGet("toi")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Toi()
        => Ok(PhanHoi.Ok(new ThongTinNguoiDungResponse(
            User.Identity?.Name ?? string.Empty,
            User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty)));

    [HttpPost("/api/nguoi-dung")]
    [Authorize(Roles = "QuanTri")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(PhanHoi<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PhanHoi<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> TaoNguoiDung(
        [FromBody] TaoNguoiDungRequest? request,
        CancellationToken ct)
    {
        if (request is null || !ModelState.IsValid)
            return BadRequest(PhanHoi.Loi("Thông tin tài khoản không hợp lệ"));

        var nguoiDung = await service.TaoNguoiDungAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, PhanHoi.Ok(
            new TaoNguoiDungResponse(nguoiDung.Id, nguoiDung.TenDangNhap, nguoiDung.VaiTro)));
    }
}
