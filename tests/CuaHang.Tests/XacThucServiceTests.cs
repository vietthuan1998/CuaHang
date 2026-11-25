using System.IdentityModel.Tokens.Jwt;
using System.Text;
using CuaHang.Api.Data;
using CuaHang.Api.Dtos;
using CuaHang.Api.Entities;
using CuaHang.Api.Security;
using CuaHang.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CuaHang.Tests;

public sealed class XacThucServiceTests
{
    [Fact]
    public async Task DangNhap_HopLe_PhatTokenHopLeVaClaimQuyen()
    {
        await using var context = TaoContext();
        var hasher = new PasswordHasher<NguoiDung>();
        var user = new NguoiDung
        {
            TenDangNhap = "quantri",
            VaiTro = "QuanTri",
            HoatDong = true
        };
        user.MatKhauBam = hasher.HashPassword(user, "mat-khau-thu-nghiem-dai");
        context.NguoiDungs.Add(user);
        await context.SaveChangesAsync();
        var config = new JwtCauHinh
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            SigningKey = "test-signing-key-that-is-at-least-32-bytes",
            ThoiHanPhut = 15
        };
        var service = new XacThucService(context, hasher, Options.Create(config));

        var result = await service.DangNhapAsync(
            new DangNhapRequest("quantri", "mat-khau-thu-nghiem-dai"));

        Assert.NotNull(result);
        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(result.Token, new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = config.Issuer,
            ValidateAudience = true,
            ValidAudience = config.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.SigningKey))
        }, out _);
        Assert.Equal("QuanTri", principal.FindFirst(
            System.Security.Claims.ClaimTypes.Role)?.Value);
        Assert.Equal("sua_san_pham", principal.FindFirst("quyen")?.Value);
    }

    [Fact]
    public async Task DangNhap_MatKhauSai_TraNull()
    {
        await using var context = TaoContext();
        var hasher = new PasswordHasher<NguoiDung>();
        var user = new NguoiDung
        {
            TenDangNhap = "nhanvien",
            VaiTro = "NhanVien",
            HoatDong = true
        };
        user.MatKhauBam = hasher.HashPassword(user, "mat-khau-thu-nghiem-dai");
        context.NguoiDungs.Add(user);
        await context.SaveChangesAsync();
        var service = new XacThucService(
            context,
            hasher,
            Options.Create(new JwtCauHinh
            {
                Issuer = "test-issuer",
                Audience = "test-audience",
                SigningKey = "test-signing-key-that-is-at-least-32-bytes"
            }));

        var result = await service.DangNhapAsync(
            new DangNhapRequest("nhanvien", "sai"));

        Assert.Null(result);
    }

    private static AppDbContext TaoContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"auth-{Guid.NewGuid():N}")
            .Options;
        return new AppDbContext(options);
    }
}
