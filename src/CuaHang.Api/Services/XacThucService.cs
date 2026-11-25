using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CuaHang.Api.Data;
using CuaHang.Api.Dtos;
using CuaHang.Api.Entities;
using CuaHang.Api.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CuaHang.Api.Services;

public sealed class XacThucService(
    AppDbContext context,
    IPasswordHasher<NguoiDung> passwordHasher,
    IOptions<JwtCauHinh> jwtOptions)
{
    private readonly JwtCauHinh _jwt = jwtOptions.Value;

    public async Task<DangNhapResponse?> DangNhapAsync(
        DangNhapRequest request,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.TenDangNhap)
            || string.IsNullOrEmpty(request.MatKhau))
            return null;

        var nguoiDung = await context.NguoiDungs.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenDangNhap == request.TenDangNhap, ct);
        if (nguoiDung is null || !nguoiDung.HoatDong)
            return null;

        var ketQuaBam = passwordHasher.VerifyHashedPassword(
            nguoiDung, nguoiDung.MatKhauBam, request.MatKhau);
        if (ketQuaBam == PasswordVerificationResult.Failed)
            return null;

        var hetHan = DateTime.UtcNow.AddMinutes(_jwt.ThoiHanPhut);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, nguoiDung.Id.ToString()),
            new(ClaimTypes.Name, nguoiDung.TenDangNhap),
            new(ClaimTypes.Role, nguoiDung.VaiTro)
        };
        if (nguoiDung.VaiTro == "QuanTri")
            claims.Add(new Claim("quyen", "sua_san_pham"));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SigningKey)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            _jwt.Issuer,
            _jwt.Audience,
            claims,
            notBefore: DateTime.UtcNow,
            expires: hetHan,
            signingCredentials: credentials);
        return new DangNhapResponse(new JwtSecurityTokenHandler().WriteToken(token), hetHan);
    }

    public async Task<NguoiDung> TaoNguoiDungAsync(
        TaoNguoiDungRequest request,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.TenDangNhap)
            || request.TenDangNhap.Length > 100
            || string.IsNullOrWhiteSpace(request.MatKhau)
            || request.MatKhau.Length < 4
            || request.MatKhau.Length > 256
            || request.VaiTro is not ("QuanTri" or "NhanVien"))
            throw new CuaHang.Api.Common.DuLieuKhongHopLeException(
                "Thông tin tài khoản không hợp lệ");

        var tenDangNhap = request.TenDangNhap.Trim();
        if (await context.NguoiDungs.AnyAsync(x => x.TenDangNhap == tenDangNhap, ct))
            throw new CuaHang.Api.Common.XungDotException("Tên đăng nhập đã tồn tại");

        var nguoiDung = new NguoiDung
        {
            TenDangNhap = tenDangNhap,
            VaiTro = request.VaiTro
        };
        nguoiDung.MatKhauBam = passwordHasher.HashPassword(nguoiDung, request.MatKhau);
        context.NguoiDungs.Add(nguoiDung);
        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (CuaHang.Api.Common.SqlLoi.LaViPhamChiMucDuyNhat(ex))
        {
            throw new CuaHang.Api.Common.XungDotException("Tên đăng nhập đã tồn tại");
        }

        return nguoiDung;
    }
}
