using CuaHang.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CuaHang.Api.Data.Configurations;

public sealed class HopThuDiConfiguration : IEntityTypeConfiguration<HopThuDi>
{
    public void Configure(EntityTypeBuilder<HopThuDi> builder)
    {
        builder.ToTable("HopThuDis");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.LoaiSuKien).HasMaxLength(100).IsUnicode(false).IsRequired();
        builder.Property(x => x.PayloadJson).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(x => x.TrangThai).HasMaxLength(20).IsUnicode(false).IsRequired();
        builder.Property(x => x.GuiSauUtc).HasColumnType("datetime2");
        builder.Property(x => x.NgayTaoUtc).HasColumnType("datetime2");
        builder.HasIndex(x => new { x.TrangThai, x.GuiSauUtc });
    }
}
