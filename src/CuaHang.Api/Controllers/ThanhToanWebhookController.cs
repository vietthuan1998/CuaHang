using System.Text;
using CuaHang.Api.Common;
using CuaHang.Api.Dtos;
using CuaHang.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Newtonsoft.Json;

namespace CuaHang.Api.Controllers;

public sealed record ThanhToanWebhookResponse(bool DaXuLy);

[ApiController]
[Route("api/webhooks/thanh-toan")]
[AllowAnonymous]
[EnableRateLimiting("dang-nhap")]
public sealed class ThanhToanWebhookController(ThanhToanWebhookService service) : ControllerBase
{
    [HttpPost]
    [RequestSizeLimit(16_384)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PhanHoi<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PhanHoi<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Nhan(CancellationToken ct)
    {
        Request.EnableBuffering();
        using var buffer = new MemoryStream();
        await Request.Body.CopyToAsync(buffer, ct);
        if (buffer.Length > 16_384)
            return BadRequest(PhanHoi.Loi("Payload quá lớn"));

        var body = buffer.ToArray();
        var chuKy = Request.Headers["X-Payment-Signature"].FirstOrDefault();
        if (!service.KiemTraChuKy(body, chuKy))
            return Unauthorized(PhanHoi.Loi("Chữ ký không hợp lệ"));

        ThanhToanWebhookRequest? request;
        try
        {
            request = JsonConvert.DeserializeObject<ThanhToanWebhookRequest>(
                Encoding.UTF8.GetString(body),
                CauHinhJson.TaoSerializerSettings());
        }
        catch (JsonException)
        {
            return BadRequest(PhanHoi.Loi("Payload không hợp lệ"));
        }
        if (request is null)
            return BadRequest(PhanHoi.Loi("Payload không hợp lệ"));

        var created = await service.XuLyAsync(request, ct);
        return Ok(PhanHoi.Ok(new ThanhToanWebhookResponse(created)));
    }
}
