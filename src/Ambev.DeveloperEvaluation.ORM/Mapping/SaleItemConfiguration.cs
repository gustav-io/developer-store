using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("SaleItems");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnType("uuid").HasDefaultValueSql("gen_random_uuid()");

        builder.OwnsOne(i => i.Product, product =>
        {
            product.Property(p => p.Id).HasColumnName("ProductId").IsRequired();
            product.Property(p => p.Name).HasColumnName("ProductName").IsRequired().HasMaxLength(200);
            product.Property(p => p.UnitPrice).HasColumnName("UnitPrice").HasPrecision(18, 2);
        });
        builder.Navigation(i => i.Product).IsRequired();

        builder.Property(i => i.Quantity).IsRequired();
        builder.Property(i => i.DiscountPercent).HasPrecision(5, 4);
        builder.Property(i => i.DiscountAmount).HasPrecision(18, 2);
        builder.Property(i => i.TotalAmount).HasPrecision(18, 2);
        builder.Property(i => i.IsCancelled).IsRequired();

        builder.HasIndex(i => i.SaleId);
    }
}
