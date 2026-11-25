using CuaHang.Api.Common;
using CuaHang.Api.Data;
using CuaHang.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace CuaHang.Api.Services;

public sealed class SanPhamAnhService(
    AppDbContext context,
    IWebHostEnvironment environment,
    IConfiguration configuration,
    ILogger<SanPhamAnhService> logger)
{
    private const long KichThuocToiDa = 5 * 1024 * 1024;
    private readonly string _thuMuc = Path.GetFullPath(
        configuration["Storage:ProductImagePath"] is { Length: > 0 } configuredPath
            ? Path.IsPathRooted(configuredPath)
                ? configuredPath
                : Path.Combine(environment.ContentRootPath, configuredPath)
            : Path.Combine(environment.ContentRootPath, "App_Data", "product-images"));

    public async Task LuuAnhAsync(int sanPhamId, IFormFile tep, CancellationToken ct)
    {
        if (tep.Length is <= 0 or > KichThuocToiDa)
            throw new DuLieuKhongHopLeException("Ảnh phải có kích thước từ 1 byte đến 5 MB");

        var sanPham = await context.SanPhams
            .FirstOrDefaultAsync(x => x.Id == sanPhamId, ct)
            ?? throw new KhongTimThayException("Không tìm thấy sản phẩm");
        var duoiTep = await NhanDangAnhAsync(tep, ct);
        var tenTep = $"{Guid.NewGuid():N}{duoiTep}";
        Directory.CreateDirectory(_thuMuc);

        var tenTepCu = sanPham.AnhTenTep;
        try
        {
            await using (var input = tep.OpenReadStream())
            await using (var output = new FileStream(
                DuongDan(tenTep), FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                await input.CopyToAsync(output, ct);
            }

            sanPham.AnhTenTep = tenTep;
            await context.SaveChangesAsync(ct);
        }
        catch
        {
            if (File.Exists(DuongDan(tenTep)))
            {
                try
                {
                    File.Delete(DuongDan(tenTep));
                }
                catch (IOException cleanupException)
                {
                    logger.LogError(cleanupException, "Không xóa được tệp ảnh tạm {TenTep}", tenTep);
                }
            }
            throw;
        }

        if (tenTepCu is not null)
        {
            try
            {
                XoaAnh(tenTepCu);
            }
            catch (IOException cleanupException)
            {
                logger.LogWarning(cleanupException, "Không xóa được ảnh cũ {TenTep}", tenTepCu);
            }
        }
    }

    public async Task<(FileStream Stream, string ContentType)> MoAnhAsync(
        int sanPhamId, CancellationToken ct)
    {
        var tenTep = await context.SanPhams.AsNoTracking()
            .Where(x => x.Id == sanPhamId)
            .Select(x => x.AnhTenTep)
            .FirstOrDefaultAsync(ct)
            ?? throw new KhongTimThayException("Không tìm thấy ảnh sản phẩm");

        var contentType = Path.GetExtension(tenTep) switch
        {
            ".jpg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => throw new InvalidOperationException("Phần mở rộng ảnh đã lưu không hợp lệ")
        };
        try
        {
            return (new FileStream(DuongDan(tenTep), FileMode.Open, FileAccess.Read, FileShare.Read), contentType);
        }
        catch (FileNotFoundException)
        {
            throw new KhongTimThayException("Không tìm thấy tệp ảnh sản phẩm");
        }
    }

    private static async Task<string> NhanDangAnhAsync(
        IFormFile tep, CancellationToken ct)
    {
        var header = new byte[12];
        await using var input = tep.OpenReadStream();
        var count = await input.ReadAsync(header, ct);
        var loai = tep.ContentType switch
        {
            "image/jpeg" when count >= 3
                && header[0] == 0xff && header[1] == 0xd8 && header[2] == 0xff
                => (".jpg", "image/jpeg"),
            "image/png" when count >= 8
                && header.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })
                => (".png", "image/png"),
            "image/webp" when count >= 12
                && header.AsSpan(0, 4).SequenceEqual("RIFF"u8)
                && header.AsSpan(8, 4).SequenceEqual("WEBP"u8)
                => (".webp", "image/webp"),
            _ => throw new DuLieuKhongHopLeException("Chỉ chấp nhận ảnh JPEG, PNG hoặc WebP hợp lệ")
        };
        return loai.Item1;
    }

    private string DuongDan(string tenTep) => Path.Combine(_thuMuc, tenTep);

    private void XoaAnh(string tenTep)
    {
        var path = DuongDan(tenTep);
        if (File.Exists(path))
            File.Delete(path);
    }
}
