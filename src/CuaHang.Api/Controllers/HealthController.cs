using CuaHang.Api.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CuaHang.Api.Controllers;

public sealed record HealthResponse(string TrangThai, DateTimeOffset ThoiGianUtc);

[ApiController]
[Route("api/health")]
[AllowAnonymous]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    // [ProducesResponseType(typeof(PhanHoi<HealthResponse>), StatusCodes.Status200OK)]
    public IActionResult Get()
        => Ok(PhanHoi.Ok(new HealthResponse("Song", DateTimeOffset.UtcNow)));
}
