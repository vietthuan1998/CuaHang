using CuaHang.Api.Data;
using CuaHang.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace CuaHang.Api.Background;

public sealed class DongBoService(
    IServiceScopeFactory scopeFactory,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<DongBoService> logger) : BackgroundService
{
    private static readonly TimeSpan ChuKy = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(ChuKy);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (configuration.GetValue<bool>("TinhNang:BatDongBo"))
                    await ChayMotVongAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Vòng đồng bộ gặp lỗi; sẽ thử lại ở chu kỳ tiếp theo");
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

    private async Task ChayMotVongAsync(CancellationToken ct)
    {
        var uriText = configuration["DichVuNgoai:DiaChi"];
        if (!Uri.TryCreate(uriText, UriKind.Absolute, out var uri)
            || uri.Scheme is not ("http" or "https"))
        {
            logger.LogError("Không chạy đồng bộ vì DichVuNgoai:DiaChi không hợp lệ");
            return;
        }

        var ky = new DateTime(
            DateTime.UtcNow.Year,
            DateTime.UtcNow.Month,
            DateTime.UtcNow.Day,
            DateTime.UtcNow.Hour,
            DateTime.UtcNow.Minute,
            0,
            DateTimeKind.Utc);

        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var run = await db.DongBoLichSus.FirstOrDefaultAsync(x => x.KyBatDauUtc == ky, ct);
        if (run is not null)
            return;

        run = new DongBoLichSu { KyBatDauUtc = ky };
        db.DongBoLichSus.Add(run);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            logger.LogInformation(ex, "Vòng đồng bộ {KyBatDauUtc} đã được tiến trình khác tạo", ky);
            return;
        }

        var client = httpClientFactory.CreateClient("DichVuNgoai");
        for (var lanThu = 1; lanThu <= 3; lanThu++)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, uri);
                request.Headers.TryAddWithoutValidation("Idempotency-Key", ky.ToString("O"));
                using var response = await client.SendAsync(request, ct);
                run.MaTrangThaiNgoai = (int)response.StatusCode;
                run.TrangThai = response.IsSuccessStatusCode ? "HoanTat" : "ThatBai";
                run.NgayCapNhatUtc = DateTime.UtcNow;
                await db.SaveChangesAsync(ct);
                if (response.IsSuccessStatusCode)
                    logger.LogInformation("Đồng bộ hoàn tất cho kỳ {KyBatDauUtc}", ky);
                else
                    logger.LogWarning(
                        "Dịch vụ ngoài trả mã {MaTrangThai} cho kỳ {KyBatDauUtc}",
                        response.StatusCode,
                        ky);
                return;
            }
            catch (HttpRequestException ex) when (lanThu < 3)
            {
                logger.LogWarning(ex, "Lần gọi dịch vụ ngoài {LanThu} thất bại", lanThu);
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, lanThu)), ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                run.TrangThai = "ThatBai";
                run.NgayCapNhatUtc = DateTime.UtcNow;
                await db.SaveChangesAsync(CancellationToken.None);
                logger.LogError(ex, "Vòng đồng bộ {KyBatDauUtc} thất bại", ky);
                return;
            }
        }
    }
}
