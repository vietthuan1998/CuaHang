using System;
using CuaHang.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CuaHang.Api.Data.Configurations;

public class NhatKyConfiguration : IEntityTypeConfiguration<NhatKy>
{
  public void Configure(EntityTypeBuilder<NhatKy> builder)
  {
    builder.ToTable("NhatKyThaoTacs");
    builder.HasKey(x => x.Id);
    builder.Property(o => o.NguoiThucHien).HasMaxLength(50);
    builder.Property(o => o.HanhDong).HasMaxLength(50).IsUnicode(false);
    builder.Property(o => o.TenBang).HasMaxLength(50).IsUnicode(false);
    builder.Property(o => o.BanGhiId).HasMaxLength(50).IsUnicode(false);
    builder.Property(o => o.NgayTao).HasColumnType("datetime2");
    builder.HasIndex(x => new { x.TenBang, x.BanGhiId, x.NgayTao });



  }
}
