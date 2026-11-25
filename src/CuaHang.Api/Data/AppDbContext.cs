using System.Security.Claims;
using System.Text.Json;
using CuaHang.Api.Entities;
using CuaHang.Api.Reporting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;

namespace CuaHang.Api.Data;

public class AppDbContext : DbContext
{
  private readonly IHttpContextAccessor? _httpContextAccessor;

  public AppDbContext(
    DbContextOptions<AppDbContext> options,
    IHttpContextAccessor? httpContextAccessor = null) : base(options)
  {
    _httpContextAccessor = httpContextAccessor;
  }

  protected AppDbContext()
  {
  }
  public DbSet<DanhMuc> DanhMucs => Set<DanhMuc>();
  public DbSet<KhachHang> KhachHangs => Set<KhachHang>();
  public DbSet<SanPham> SanPhams => Set<SanPham>();
  public DbSet<DonHang> DonHangs => Set<DonHang>();
  public DbSet<ChiTietDonHang> ChiTietDonHangs => Set<ChiTietDonHang>();
  public DbSet<NhatKy> NhatKys => Set<NhatKy>();
  public DbSet<BaoCaoSanPhamThang> BaoCaoSanPhamThangs => Set<BaoCaoSanPhamThang>();
  public DbSet<NguoiDung> NguoiDungs => Set<NguoiDung>();
  public DbSet<DongBoLichSu> DongBoLichSus => Set<DongBoLichSu>();
  public DbSet<ThanhToanWebhook> ThanhToanWebhooks => Set<ThanhToanWebhook>();
  public DbSet<HopThuDi> HopThuDis => Set<HopThuDi>();

  public override int SaveChanges()
  {
    var thayDoi = ChupThayDoi();
    if (thayDoi.Count == 0)
      return base.SaveChanges();

    using var transaction = BatDauTransactionNeuCan();
    var soDong = base.SaveChanges();
    NhatKys.AddRange(TaoNhatKy(thayDoi));
    base.SaveChanges();
    transaction?.Commit();
    return soDong;
  }

  public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
  {
    var thayDoi = ChupThayDoi();
    if (thayDoi.Count == 0)
      return await base.SaveChangesAsync(ct);

    await using var transaction = await BatDauTransactionNeuCanAsync(ct);
    var soDong = await base.SaveChangesAsync(ct);
    NhatKys.AddRange(TaoNhatKy(thayDoi));
    await base.SaveChangesAsync(ct);
    if (transaction is not null)
      await transaction.CommitAsync(ct);
    return soDong;
  }

  private List<ThayDoiCanGhi> ChupThayDoi()
  {
    ChangeTracker.DetectChanges();

    var nguoiThucHien = LayNguoiThucHien();
    foreach (var entry in ChangeTracker.Entries().Where(e => e.Entity is SanPham or DonHang))
    {
      if (entry.State == EntityState.Added)
      {
        entry.Property("NgayTao").CurrentValue ??= DateTime.UtcNow;
        entry.Property("NguoiTao").CurrentValue ??= nguoiThucHien;
      }
      else if (entry.State == EntityState.Modified)
      {
        entry.Property("NgayCapNhat").CurrentValue = DateTime.UtcNow;
        entry.Property("NguoiCapNhat").CurrentValue = nguoiThucHien;
        entry.Property("NgayTao").IsModified = false;
        entry.Property("NguoiTao").IsModified = false;
      }
    }

    return ChangeTracker.Entries()
      .Where(e => e.Entity is SanPham or DonHang &&
        e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
      .Where(e => e.State != EntityState.Modified || e.Properties.Any(p => p.IsModified))
      .Select(e => new ThayDoiCanGhi(
        e,
        e.State,
        e.Metadata.GetTableName() ?? e.Metadata.ClrType.Name,
        e.State is EntityState.Modified or EntityState.Deleted
          ? JsonSerializer.Serialize(LayGiaTri(e, original: true))
          : string.Empty,
        LayKhoaChinh(e),
        LaXoaMem(e)))
      .ToList();
  }

  private IEnumerable<NhatKy> TaoNhatKy(IEnumerable<ThayDoiCanGhi> thayDoi)
  {
    var nguoiThucHien = LayNguoiThucHien();

    return thayDoi.Select(change => new NhatKy
    {
      NguoiThucHien = nguoiThucHien,
      HanhDong = change.HanhDong,
      TenBang = change.TenBang,
      BanGhiId = change.TrangThai == EntityState.Added
        ? LayKhoaChinh(change.Entry)
        : change.BanGhiId,
      GiaTriTruoc = change.GiaTriTruoc,
      GiaTriSau = change.TrangThai == EntityState.Deleted
        ? string.Empty
        : JsonSerializer.Serialize(LayGiaTri(change.Entry)),
      NgayTao = DateTime.UtcNow
    });
  }

  private string LayNguoiThucHien()
  {
    var nguoiThucHien = _httpContextAccessor?.HttpContext?.User.Identity?.Name;
    if (string.IsNullOrWhiteSpace(nguoiThucHien))
      nguoiThucHien = _httpContextAccessor?.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    return string.IsNullOrWhiteSpace(nguoiThucHien) ? "system" : nguoiThucHien;
  }

  private static Dictionary<string, object?> LayGiaTri(EntityEntry entry, bool original = false)
    => entry.Properties
      .Where(p => !p.Metadata.IsShadowProperty() && !LaDuLieuNhayCam(p.Metadata.Name))
      .ToDictionary(
        p => p.Metadata.Name,
        p => original ? p.OriginalValue : p.CurrentValue);

  private static bool LaDuLieuNhayCam(string tenThuocTinh)
    => tenThuocTinh.Contains("Password", StringComparison.OrdinalIgnoreCase)
      || tenThuocTinh.Contains("Token", StringComparison.OrdinalIgnoreCase)
      || tenThuocTinh.Contains("Secret", StringComparison.OrdinalIgnoreCase)
      || tenThuocTinh.Equals("Email", StringComparison.OrdinalIgnoreCase)
      || tenThuocTinh.Equals("SoDienThoai", StringComparison.OrdinalIgnoreCase);

  private static string LayKhoaChinh(EntityEntry entry)
  {
    var khoa = entry.Metadata.FindPrimaryKey();
    if (khoa is null)
      return string.Empty;

    return string.Join(",", khoa.Properties.Select(p =>
      entry.Property(p.Name).CurrentValue?.ToString() ?? string.Empty));
  }

  private static bool LaXoaMem(EntityEntry entry)
  {
    if (entry.Entity is not SanPham || entry.State != EntityState.Modified)
      return false;

    var daXoa = entry.Property(nameof(SanPham.DaXoa));
    return daXoa.IsModified && daXoa.OriginalValue is false && daXoa.CurrentValue is true;
  }

  private IDbContextTransaction? BatDauTransactionNeuCan()
    => Database.IsRelational() && Database.CurrentTransaction is null
      ? Database.BeginTransaction()
      : null;

  private Task<IDbContextTransaction?> BatDauTransactionNeuCanAsync(CancellationToken ct)
    => Database.IsRelational() && Database.CurrentTransaction is null
      ? BatDauTransactionAsync(ct)
      : Task.FromResult<IDbContextTransaction?>(null);

  private async Task<IDbContextTransaction?> BatDauTransactionAsync(CancellationToken ct)
    => await Database.BeginTransactionAsync(ct);

  private sealed record ThayDoiCanGhi(
    EntityEntry Entry,
    EntityState TrangThai,
    string TenBang,
    string GiaTriTruoc,
    string BanGhiId,
    bool XoaMem)
  {
    public string HanhDong => XoaMem ? "XoaMem" : TrangThai switch
    {
      EntityState.Added => "Them",
      EntityState.Modified => "Sua",
      EntityState.Deleted => "Xoa",
      _ => throw new InvalidOperationException($"Trạng thái không hỗ trợ: {TrangThai}")
    };
  }

  protected override void OnModelCreating(ModelBuilder mb)
  {
    mb.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    mb.Entity<BaoCaoSanPhamThang>()
      .HasNoKey()
      .ToView("vw_BaoCaoSanPhamThang");
    mb.Entity<BaoCaoSanPhamThang>()
      .Property(x => x.DoanhSo)
      .HasColumnType("decimal(38,2)");
  }
}
