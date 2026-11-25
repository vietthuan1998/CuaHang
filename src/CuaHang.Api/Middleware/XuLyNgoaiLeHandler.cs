using CuaHang.Api.Common;
using Microsoft.AspNetCore.Diagnostics;
using Newtonsoft.Json;

namespace CuaHang.Api.Middleware;

public sealed class XuLyNgoaiLeHandler : IExceptionHandler
{
    private readonly ILogger<XuLyNgoaiLeHandler> _logger;

    public XuLyNgoaiLeHandler(ILogger<XuLyNgoaiLeHandler> logger) => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (context.Response.HasStarted)
            return false;

        var (status, message) = exception switch
        {
            XungDotException => (StatusCodes.Status409Conflict, exception.Message),
            HetHangException => (StatusCodes.Status409Conflict, exception.Message),
            DuLieuKhongHopLeException => (StatusCodes.Status400BadRequest, exception.Message),
            KhongTimThayException => (StatusCodes.Status404NotFound, exception.Message),
            _ => (StatusCodes.Status500InternalServerError, "Lỗi hệ thống")
        };

        if (status >= StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Lỗi không xử lý được tại {DuongDan}", context.Request.Path);
        else
            _logger.LogWarning(
                "Yêu cầu thất bại với mã {MaTrangThai} tại {DuongDan}: {ThongBao}",
                status,
                context.Request.Path,
                message);

        context.Response.Clear();
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json; charset=utf-8";
        var json = JsonConvert.SerializeObject(PhanHoi.Loi(message), CauHinhJson.TaoSerializerSettings());
        await context.Response.WriteAsync(json, cancellationToken);
        return true;
    }
}
