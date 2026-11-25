using System.Net;
using CuaHang.Api.Data;
using CuaHang.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace CuaHang.Api.Background;

public sealed class HopThuDiService(
    IServiceScopeFactory scopeFactory,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<HopThuDiService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (configuration.GetValue<bool>("TinhNang:GuiHopThuDi"))
                    await GuiMotLoAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Xử lý hộp thư đi gặp lỗi; sẽ thử lại ở chu kỳ tiếp theo");
            }
            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task GuiMotLoAsync(CancellationToken ct)
    {
        var uriText = configuration["DichVuNgoai:WebhookUrl"];
        if (!Uri.TryCreate(uriText, UriKind.Absolute, out var uri)
            || uri.Scheme is not ("http" or "https"))
        {
            logger.LogError("Không gửi hộp thư đi vì DichVuNgoai:WebhookUrl không hợp lệ");
            return;
        }

        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var messages = await db.HopThuDis
            .Where(x => x.TrangThai == "ChoGui"
                && x.GuiSauUtc <= DateTime.UtcNow
                && x.SoLanThu < 8)
            .OrderBy(x => x.Id)
            .Take(20)
            .ToListAsync(ct);

        var client = httpClientFactory.CreateClient("HopThuDi");
        foreach (var message in messages)
        {
            await GuiAsync(client, uri, db, message, ct);
        }
    }

    private async Task GuiAsync(
        HttpClient client,
        Uri uri,
        AppDbContext db,
        HopThuDi message,
        CancellationToken ct)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new StringContent(
                    message.PayloadJson,
                    System.Text.Encoding.UTF8,
                    "application/json")
            };
            request.Headers.TryAddWithoutValidation("Idempotency-Key", message.Id.ToString());
            using var response = await client.SendAsync(request, ct);
            message.SoLanThu++;
            if (response.IsSuccessStatusCode)
            {
                message.TrangThai = "DaGui";
            }
            else
            {
                var loiVinhVien = response.StatusCode is >= HttpStatusCode.BadRequest
                    and < HttpStatusCode.InternalServerError;
                message.TrangThai = loiVinhVien || message.SoLanThu >= 8
                    ? "ThatBai"
                    : "ChoGui";
                if (message.TrangThai == "ChoGui")
                    message.GuiSauUtc = DateTime.UtcNow.AddSeconds(
                        Math.Min(Math.Pow(2, message.SoLanThu), 300));
            }
            await db.SaveChangesAsync(ct);
        }
        catch (HttpRequestException ex)
        {
            await GhiNhanThatBaiAsync(db, message, ex, ct);
        }
        catch (OperationCanceledException ex) when (!ct.IsCancellationRequested)
        {
            await GhiNhanThatBaiAsync(db, message, ex, ct);
        }
    }

    private async Task GhiNhanThatBaiAsync(
        AppDbContext db,
        HopThuDi message,
        Exception exception,
        CancellationToken ct)
    {
        message.SoLanThu++;
        message.TrangThai = message.SoLanThu >= 8 ? "ThatBai" : "ChoGui";
        message.GuiSauUtc = DateTime.UtcNow.AddSeconds(
            Math.Min(Math.Pow(2, message.SoLanThu), 300));
        await db.SaveChangesAsync(ct);
        logger.LogWarning(exception, "Gửi hộp thư đi {HopThuDiId} thất bại", message.Id);
    }
}
