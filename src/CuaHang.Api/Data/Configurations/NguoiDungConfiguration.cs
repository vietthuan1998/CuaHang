using CuaHang.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CuaHang.Api.Data.Configurations;

public sealed class NguoiDungConfiguration : IEntityTypeConfiguration<NguoiDung>
{
    public void Configure(EntityTypeBuilder<NguoiDung> builder)
    {
        builder.ToTable("NguoiDungs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TenDangNhap).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.TenDangNhap).IsUnique();
        builder.Property(x => x.MatKhauBam).HasMaxLength(500).IsRequired();
        builder.Property(x => x.VaiTro).HasMaxLength(30).IsUnicode(false).IsRequired();
        builder.Property(x => x.HoatDong).HasDefaultValue(true);
        builder.Property(x => x.NgayTao).HasColumnType("datetime2");
    }
}
