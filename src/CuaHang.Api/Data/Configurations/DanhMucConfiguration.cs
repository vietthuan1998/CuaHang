using System;
using CuaHang.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CuaHang.Api.Data.Configurations;

public class DanhMucConfiguration : IEntityTypeConfiguration<DanhMuc>
{
  public void Configure(EntityTypeBuilder<DanhMuc> builder)
  {
    builder.ToTable("DanhMucs");
    builder.HasKey(x => x.Id);
    builder.Property(x => x.Ten).HasMaxLength(100).IsRequired();
    builder.HasIndex(x => x.Ten).IsUnique();
  }
}
