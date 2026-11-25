using System.Globalization;
using System.Text;
using CuaHang.Api.Data;
using CuaHang.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace CuaHang.Api.Services;

public sealed class HoaDonService(AppDbContext context)
{
    public async Task<byte[]?> TaoPdfAsync(int donHangId, CancellationToken ct = default)
    {
        var donHang = await context.DonHangs.AsNoTracking()
            .Include(x => x.KhachHang)
            .Include(x => x.SanPham)
            .Include(x => x.ChiTietDonHangs)
                .ThenInclude(x => x.SanPham)
            .FirstOrDefaultAsync(x => x.Id == donHangId, ct);
        if (donHang is null)
            return null;

        var dong = new List<string>
        {
            $"HOA DON DON HANG #{donHang.Id}",
            $"Khach hang: {donHang.KhachHang?.Ten ?? "Khong ro"}",
            $"Ngay tao (UTC): {donHang.NgayTao:yyyy-MM-dd HH:mm:ss}",
            $"Trang thai: {donHang.TrangThai}",
            ""
        };
        if (donHang.ChiTietDonHangs.Count > 0)
        {
            foreach (var item in donHang.ChiTietDonHangs.OrderBy(x => x.Id))
                dong.Add($"{item.SanPham?.Ten ?? "San pham"} | SL {item.SoLuong} | " +
                    $"{item.DonGia.ToString("N2", CultureInfo.InvariantCulture)} | " +
                    $"{item.ThanhTien.ToString("N2", CultureInfo.InvariantCulture)}");
        }
        else
        {
            dong.Add($"{donHang.SanPham?.Ten ?? "San pham"} | SL {donHang.SoLuong} | " +
                $"{donHang.DonGia.ToString("N2", CultureInfo.InvariantCulture)} | " +
                $"{donHang.ThanhTien.ToString("N2", CultureInfo.InvariantCulture)}");
        }
        dong.Add("");
        dong.Add($"TONG CONG: {donHang.ThanhTien.ToString("N2", CultureInfo.InvariantCulture)}");

        return TaoPdf(dong.Select(ChuyenAscii).ToArray());
    }

    private static byte[] TaoPdf(IReadOnlyList<string> dong)
    {
        const int soDongMoiTrang = 48;
        var soTrang = Math.Max(1, (int)Math.Ceiling(dong.Count / (double)soDongMoiTrang));
        var objects = new List<string>
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            "",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>"
        };
        var kids = new List<string>();
        for (var trang = 0; trang < soTrang; trang++)
        {
            var pageObjectId = 4 + trang * 2;
            var streamObjectId = pageObjectId + 1;
            kids.Add($"{pageObjectId} 0 R");
            objects.Add(
                $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] " +
                $"/Resources << /Font << /F1 3 0 R >> >> /Contents {streamObjectId} 0 R >>");

            var content = new StringBuilder();
            var first = trang * soDongMoiTrang;
            var count = Math.Min(soDongMoiTrang, dong.Count - first);
            for (var i = 0; i < count; i++)
            {
                var text = dong[first + i].Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
                var y = 750 - i * 15;
                content.Append(CultureInfo.InvariantCulture,
                    $"BT /F1 11 Tf 50 {y} Td ({text}) Tj ET\n");
            }
            var contentBytes = Encoding.ASCII.GetBytes(content.ToString());
            objects.Add($"<< /Length {contentBytes.Length} >>\nstream\n{content}endstream");
        }
        objects[1] = $"<< /Type /Pages /Kids [{string.Join(' ', kids)}] /Count {soTrang} >>";

        using var output = new MemoryStream();
        WriteAscii(output, "%PDF-1.4\n");
        var offsets = new List<long> { 0 };
        for (var i = 0; i < objects.Count; i++)
        {
            offsets.Add(output.Position);
            WriteAscii(output, $"{i + 1} 0 obj\n{objects[i]}\nendobj\n");
        }

        var xrefOffset = output.Position;
        WriteAscii(output, $"xref\n0 {objects.Count + 1}\n0000000000 65535 f \n");
        foreach (var offset in offsets.Skip(1))
            WriteAscii(output, $"{offset:0000000000} 00000 n \n");
        WriteAscii(output,
            $"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xrefOffset}\n%%EOF");
        return output.ToArray();
    }

    private static string ChuyenAscii(string value)
    {
        var expanded = value.Replace('đ', 'd').Replace('Đ', 'D').Normalize(NormalizationForm.FormD);
        var result = new StringBuilder(expanded.Length);
        foreach (var character in expanded)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
                continue;
            result.Append(char.IsAscii(character) && !char.IsControl(character) ? character : '?');
        }
        return result.ToString();
    }

    private static void WriteAscii(Stream output, string value)
        => output.Write(Encoding.ASCII.GetBytes(value));
}
