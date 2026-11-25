using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CuaHang.Api.Hubs;

[Authorize]
public sealed class ThongBaoHub : Hub
{
    public const string NhomQuanTri = "quan-tri";

    public override async Task OnConnectedAsync()
    {
        if (Context.User?.IsInRole("QuanTri") == true)
            await Groups.AddToGroupAsync(Context.ConnectionId, NhomQuanTri);
        await base.OnConnectedAsync();
    }
}
