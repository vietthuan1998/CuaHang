using CuaHang.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CuaHang.Api.Services;

public sealed class ThongBaoService(
    IHubContext<ThongBaoHub> hubContext,
    ILogger<ThongBaoService> logger)
{
    public async Task DonHangMoiAsync(int donHangId, CancellationToken ct = default)
    {
        try
        {
            await hubContext.Clients.Group(ThongBaoHub.NhomQuanTri)
                .SendAsync("DonHangMoi", new { id = donHangId }, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Không gửi được thông báo cho đơn hàng {DonHangId}", donHangId);
        }
    }
}
