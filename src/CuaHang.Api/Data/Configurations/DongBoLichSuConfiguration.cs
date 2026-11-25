using CuaHang.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CuaHang.Api.Data.Configurations;

public sealed class DongBoLichSuConfiguration : IEntityTypeConfiguration<DongBoLichSu>
{
    public void Configure(EntityTypeBuilder<DongBoLichSu> builder)
    {
        builder.ToTable("DongBoLichSus");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.KyBatDauUtc).IsUnique();
        builder.Property(x => x.TrangThai).HasMaxLength(20).IsUnicode(false).IsRequired();
        builder.Property(x => x.KyBatDauUtc).HasColumnType("datetime2");
        builder.Property(x => x.NgayCapNhatUtc).HasColumnType("datetime2");
    }
}
