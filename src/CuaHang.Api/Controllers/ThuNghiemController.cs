using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CuaHang.Api.Controllers;

[ApiController]
[Route("api/thu-nghiem")]
[AllowAnonymous]
public sealed class ThuNghiemController(IWebHostEnvironment environment) : ControllerBase
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpGet("chao")]
    public IActionResult Chao() => environment.IsDevelopment() ? Ok("xin chao") : NotFound();

    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpGet("lap-lai")]
    public IActionResult LapLai([FromQuery] string? noiDung)
        => environment.IsDevelopment() ? Ok(noiDung ?? string.Empty) : NotFound();
}
