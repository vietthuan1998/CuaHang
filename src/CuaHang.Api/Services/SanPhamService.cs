using System;
using CuaHang.Api.Common;
using CuaHang.Api.Data;
using CuaHang.Api.Dtos;
using CuaHang.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace CuaHang.Api.Services;

public class SanPhamService
{
  private const string ThongBaoTrung = "Sản phẩm đã tồn tại trong danh mục này";

  private readonly AppDbContext _context;

  public SanPhamService(AppDbContext context) => _context = context;

  public async Task<SanPhamResponse?> LayTheoMaAsync(int id, CancellationToken ct = default)
  {
    var result = await _context.SanPhams
    .AsNoTracking()
    .Where(x => x.Id == id)
    .Select(x => new SanPhamResponse(
      x.Id, x.DanhMucId, x.Ten, x.DonGia, x.TonKho, x.GhiChu, x.RowVersion,
      x.AnhTenTep == null ? null : $"/api/san-pham/{x.Id}/anh"
    ))
    .FirstOrDefaultAsync(ct);

    return result;
  }

  public async Task<PaginateResponse<SanPhamResponse>> TimAsync(
    string? tuKhoa,
    int? danhMucId,
    string sapXep,
    int page,
    int pageSize,
    CancellationToken ct = default)
  {
    var query = _context.SanPhams.AsNoTracking();
    if (!string.IsNullOrWhiteSpace(tuKhoa))
    {
      var tuKhoaDaCat = tuKhoa.Trim();
      query = query.Where(x => x.Ten.Contains(tuKhoaDaCat));
    }

    if (danhMucId.HasValue)
      query = query.Where(x => x.DanhMucId == danhMucId.Value);

    var tongSo = await query.CountAsync(ct);
    var coThuTu = sapXep switch
    {
      "ten" => query.OrderBy(x => x.Ten).ThenBy(x => x.Id),
      "-ten" => query.OrderByDescending(x => x.Ten).ThenBy(x => x.Id),
      "donGia" => query.OrderBy(x => x.DonGia).ThenBy(x => x.Id),
      "-donGia" => query.OrderByDescending(x => x.DonGia).ThenBy(x => x.Id),
      _ => throw new ArgumentOutOfRangeException(nameof(sapXep))
    };

    var danhSach = await coThuTu
      .Skip((int)((long)(page - 1) * pageSize))
      .Take(pageSize)
      .Select(x => new SanPhamResponse(
        x.Id, x.DanhMucId, x.Ten, x.DonGia, x.TonKho, x.GhiChu, x.RowVersion,
        x.AnhTenTep == null ? null : $"/api/san-pham/{x.Id}/anh"))
      .ToListAsync(ct);

    return new PaginateResponse<SanPhamResponse>(danhSach, page, pageSize, tongSo);
  }

  public async Task<IReadOnlyList<SanPhamResponse>> TimTonThapAsync(
    int nguong,
    CancellationToken ct = default)
    => await _context.SanPhams
      .AsNoTracking()
      .Where(x => x.TonKho <= nguong)
      .OrderBy(x => x.TonKho)
      .ThenBy(x => x.Id)
      .Select(x => new SanPhamResponse(
        x.Id, x.DanhMucId, x.Ten, x.DonGia, x.TonKho, x.GhiChu, x.RowVersion,
        x.AnhTenTep == null ? null : $"/api/san-pham/{x.Id}/anh"))
      .ToListAsync(ct);

  public async Task<SanPhamResponse> CapNhatTonKhoAsync(
    int id,
    int tonKho,
    byte[] rowVersion,
    CancellationToken ct = default)
  {
    var sanPham = await _context.SanPhams.FirstOrDefaultAsync(x => x.Id == id, ct)
      ?? throw new KhongTimThayException("Không tìm thấy sản phẩm");
    if (!sanPham.RowVersion.SequenceEqual(rowVersion))
      throw new XungDotException("Sản phẩm đã được cập nhật");

    _context.Entry(sanPham).Property(x => x.RowVersion).OriginalValue = rowVersion;
    sanPham.TonKho = tonKho;
    try
    {
      await _context.SaveChangesAsync(ct);
    }
    catch (DbUpdateConcurrencyException)
    {
      throw new XungDotException("Sản phẩm vừa được cập nhật");
    }

    return new SanPhamResponse(
      sanPham.Id,
      sanPham.DanhMucId,
      sanPham.Ten,
      sanPham.DonGia,
      sanPham.TonKho,
      sanPham.GhiChu,
      sanPham.RowVersion,
      sanPham.AnhTenTep is null ? null : $"/api/san-pham/{sanPham.Id}/anh");
  }

  public async Task<SanPhamResponse> TaoSanPhamAsync(SanPhamRequest sp, CancellationToken ct = default)
  {
    var ten = sp.Ten ?? throw Thieu("ten");
    var danhMucId = sp.DanhMucId ?? throw Thieu("DanhMucId");
    var donGia = sp.DonGia ?? throw Thieu("donGia");
    var tonKho = sp.TonKho ?? throw Thieu("tonKho");

    var daTonTai = await _context.SanPhams
      .AsNoTracking()
      .AnyAsync(x => x.DanhMucId == danhMucId && x.Ten == ten, ct);
    if (daTonTai) throw new XungDotException(ThongBaoTrung);

    var data = new SanPham
    {
      Ten = ten,
      DanhMucId = danhMucId,
      DonGia = donGia,
      TonKho = tonKho,
      GhiChu = sp.GhiChu
    };
    _context.SanPhams.Add(data);

    try
    {
      await _context.SaveChangesAsync(ct);
    }
    catch (DbUpdateException ex) when (SqlLoi.LaViPhamChiMucDuyNhat(ex))
    {
      throw new XungDotException(ThongBaoTrung);
    }

    return new SanPhamResponse(
      data.Id,
      data.DanhMucId,
      data.Ten,
      data.DonGia,
      data.TonKho,
      data.GhiChu,
      data.RowVersion,
      data.AnhTenTep is null ? null : $"/api/san-pham/{data.Id}/anh"
    );
  }
  public Task<bool> DanhMucTonTai(int id, CancellationToken ct = default)
    => _context.DanhMucs.AsNoTracking().AnyAsync(x => x.Id == id, ct);

  public async Task<bool> XoaSanPham(int id, CancellationToken ct = default)
  {
    var sp = await _context.SanPhams.FirstOrDefaultAsync(x => x.Id == id, ct);
    if (sp is null) return false;
    sp.DaXoa = true;
    await _context.SaveChangesAsync(ct);
    return true;
  }

  private static InvalidOperationException Thieu(string truong)
        => new($"Đã qua kiểm tra nhưng thiếu {truong}");
}
