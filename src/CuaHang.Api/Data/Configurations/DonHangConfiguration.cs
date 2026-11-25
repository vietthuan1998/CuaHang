using System;
using CuaHang.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CuaHang.Api.Data.Configurations;

public class DonHangConfiguration : IEntityTypeConfiguration<DonHang>
{
  public void Configure(EntityTypeBuilder<DonHang> builder)
  {
    builder.ToTable("DonHangs", t =>
    {
      t.HasCheckConstraint("CK_DonHangs_SoLuong", "[SoLuong] > 0");
    });
    builder.HasKey(dh => dh.Id);
    builder.Property(t => t.ThanhTien).HasColumnType("decimal(18,2)");
    builder.Property(t => t.DonGia).HasColumnType("decimal(18,2)");
    builder.Property(t => t.TrangThai).HasConversion<string>().HasMaxLength(20).IsRequired().IsUnicode(false);
    builder.Property(t => t.NgayTao).HasColumnType("datetime2");
    builder.Property(t => t.NgayCapNhat).HasColumnType("datetime2");
    builder.Property(t => t.NguoiTao).HasMaxLength(50);
    builder.Property(t => t.NguoiCapNhat).HasMaxLength(50);
    builder.Property(t => t.IdempotencyKey).HasMaxLength(100).IsUnicode(false);
    builder.HasIndex(t => t.IdempotencyKey).IsUnique().HasFilter("[IdempotencyKey] IS NOT NULL");
    builder.HasOne(dh => dh.KhachHang)
      .WithMany()
      .HasForeignKey(dh => dh.KhachHangId)
      .OnDelete(DeleteBehavior.Restrict);
    builder.HasOne(dh => dh.SanPham)
      .WithMany()
      .HasForeignKey(dh => dh.SanPhamId)
      .IsRequired(false)
      .OnDelete(DeleteBehavior.Restrict);
  }
}
