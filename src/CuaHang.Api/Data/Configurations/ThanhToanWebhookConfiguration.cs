using CuaHang.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CuaHang.Api.Data.Configurations;

public sealed class ThanhToanWebhookConfiguration : IEntityTypeConfiguration<ThanhToanWebhook>
{
    public void Configure(EntityTypeBuilder<ThanhToanWebhook> builder)
    {
        builder.ToTable("ThanhToanWebhooks");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.MaThanhToanNgoai).HasMaxLength(120).IsUnicode(false).IsRequired();
        builder.HasIndex(x => x.MaThanhToanNgoai).IsUnique();
        builder.HasOne<DonHang>().WithMany().HasForeignKey(x => x.DonHangId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.NgayNhanUtc).HasColumnType("datetime2");
    }
}
