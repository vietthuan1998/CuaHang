using System;
using CuaHang.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CuaHang.Api.Data.Configurations;

public class KhachHangConfiguration : IEntityTypeConfiguration<KhachHang>
{
  public void Configure(EntityTypeBuilder<KhachHang> builder)
  {
    builder.ToTable("KhachHangs");
    builder.HasKey(kh => kh.Id);
    builder.Property(kh => kh.Ten).HasMaxLength(200).IsRequired();
    builder.Property(kh => kh.SoDienThoai).HasMaxLength(20).IsRequired().IsUnicode(false);
    builder.HasIndex(kh => kh.SoDienThoai).IsUnique();
    builder.Property(kh => kh.Email).HasMaxLength(256).IsUnicode(false);
    builder.Property(kh => kh.NgayTao).HasColumnType("datetime2");

  }
}
