using CuaHang.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CuaHang.Api.Data.Configurations;

public sealed class ChiTietDonHangConfiguration : IEntityTypeConfiguration<ChiTietDonHang>
{
    public void Configure(EntityTypeBuilder<ChiTietDonHang> builder)
    {
        builder.ToTable("ChiTietDonHangs", table =>
        {
            table.HasCheckConstraint("CK_ChiTietDonHangs_SoLuong", "[SoLuong] > 0");
            table.HasCheckConstraint("CK_ChiTietDonHangs_DonGia", "[DonGia] >= 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DonGia).HasColumnType("decimal(18,2)");
        builder.Property(x => x.ThanhTien).HasColumnType("decimal(18,2)");
        builder.HasOne(x => x.DonHang)
            .WithMany(x => x.ChiTietDonHangs)
            .HasForeignKey(x => x.DonHangId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.SanPham)
            .WithMany()
            .HasForeignKey(x => x.SanPhamId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.DonHangId);
    }
}
