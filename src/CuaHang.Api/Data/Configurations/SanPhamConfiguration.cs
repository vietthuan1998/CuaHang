using System;
using CuaHang.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CuaHang.Api.Data.Configurations;

public class SanPhamConfiguration : IEntityTypeConfiguration<SanPham>
{
  public void Configure(EntityTypeBuilder<SanPham> builder)
  {
    builder.ToTable("SanPhams",
    t =>
    {
      t.HasCheckConstraint("CK_SanPhams_DonGia", "[DonGia] >= 0");
      t.HasCheckConstraint("CK_SanPhams_TonKho", "[TonKho] >= 0");
    });
    builder.HasKey(s => s.Id);
    builder.Property(s => s.Ten).HasMaxLength(200).IsRequired();
    builder.Property(s => s.DonGia).HasColumnType("decimal(18,2)");
    builder.Property(s => s.GhiChu).HasMaxLength(500);
    builder.Property(s => s.DaXoa).HasDefaultValue(false);
    builder.HasQueryFilter(s => !s.DaXoa);
    builder.Property(s => s.NgayTao).HasColumnType("datetime2");
    builder.Property(s => s.NgayCapNhat).HasColumnType("datetime2");
    builder.Property(s => s.NguoiTao).HasMaxLength(50);
    builder.Property(s => s.NguoiCapNhat).HasMaxLength(50);
    builder.Property(s => s.RowVersion).IsRowVersion();
    builder.Property(s => s.AnhTenTep).HasMaxLength(100).IsUnicode(false);
    builder.HasIndex(s => new { s.DanhMucId, s.Ten })
      .IsUnique()
      .HasFilter("[DaXoa] = 0");

    builder.HasOne(s => s.DanhMuc)
      .WithMany()
      .HasForeignKey(s => s.DanhMucId)
      .OnDelete(DeleteBehavior.Restrict);
  }
}
