using System.Security.Cryptography;
using System.Text;
using CuaHang.Api.Common;
using CuaHang.Api.Data;
using CuaHang.Api.Dtos;
using CuaHang.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CuaHang.Api.Services;

public sealed class ThanhToanWebhookService(
    AppDbContext context,
    IConfiguration configuration)
{
    public bool KiemTraChuKy(ReadOnlySpan<byte> body, string? chuKyHex)
    {
        var secret = configuration["Webhook:SigningSecret"];
        if (string.IsNullOrEmpty(secret)
            || string.IsNullOrWhiteSpace(chuKyHex)
            || chuKyHex.Length != 64)
            return false;

        byte[] chuKyNhan;
        try
        {
            chuKyNhan = Convert.FromHexString(chuKyHex);
        }
        catch (FormatException)
        {
            return false;
        }

        var chuKyDung = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), body);
        return CryptographicOperations.FixedTimeEquals(chuKyDung, chuKyNhan);
    }

    public async Task<bool> XuLyAsync(
        ThanhToanWebhookRequest request,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.MaThanhToanNgoai)
            || request.MaThanhToanNgoai.Length > 120
            || request.DonHangId is null or <= 0
            || !string.Equals(request.TrangThai, "DaThanhToan", StringComparison.Ordinal))
            throw new DuLieuKhongHopLeException("Thông tin thanh toán không hợp lệ");

        var externalId = request.MaThanhToanNgoai.Trim();
        if (await context.ThanhToanWebhooks.AsNoTracking()
                .AnyAsync(x => x.MaThanhToanNgoai == externalId, ct))
            return false;

        await using var transaction = context.Database.IsRelational()
            ? await context.Database.BeginTransactionAsync(ct)
            : null;
        var donHang = await context.DonHangs
            .FirstOrDefaultAsync(x => x.Id == request.DonHangId.Value, ct)
            ?? throw new KhongTimThayException("Không tìm thấy đơn hàng");
        if (donHang.TrangThai != TrangThaiDon.Moi)
            throw new XungDotException("Trạng thái đơn hàng không thể thanh toán");

        donHang.TrangThai = TrangThaiDon.DaThanhToan;
        context.ThanhToanWebhooks.Add(new ThanhToanWebhook
        {
            MaThanhToanNgoai = externalId,
            DonHangId = donHang.Id
        });
        context.HopThuDis.Add(new HopThuDi
        {
            LoaiSuKien = "ThanhToanDaXacNhan",
            PayloadJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                donHang.Id,
                MaThanhToanNgoai = externalId
            })
        });

        try
        {
            await context.SaveChangesAsync(ct);
            if (transaction is not null)
                await transaction.CommitAsync(ct);
        }
        catch (DbUpdateException ex) when (CuaHang.Api.Common.SqlLoi.LaViPhamChiMucDuyNhat(ex))
        {
            return false;
        }
        return true;
    }
}
