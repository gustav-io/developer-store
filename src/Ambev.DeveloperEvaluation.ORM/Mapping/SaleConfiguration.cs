using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");

        builder.HasKey(s => s.Id);
        // Identity is assigned by the aggregate (Sale.Create); a database default would make EF treat
        // navigated entities with a set key as existing rows instead of new ones.
        builder.Property(s => s.Id).HasColumnType("uuid").ValueGeneratedNever();

        builder.Property(s => s.SaleNumber)
            .HasDefaultValueSql($"nextval('\"{DefaultContext.SaleNumberSequence}\"')")
            .ValueGeneratedOnAdd();
        builder.HasIndex(s => s.SaleNumber).IsUnique();

        builder.Property(s => s.SaleDate).IsRequired();
        builder.HasIndex(s => s.SaleDate);

        builder.OwnsOne(s => s.Customer, customer =>
        {
            customer.Property(c => c.Id).HasColumnName("CustomerId").IsRequired();
            customer.Property(c => c.Name).HasColumnName("CustomerName").IsRequired().HasMaxLength(100);
            customer.HasIndex(c => c.Id);
        });
        builder.Navigation(s => s.Customer).IsRequired();

        builder.OwnsOne(s => s.Branch, branch =>
        {
            branch.Property(b => b.Id).HasColumnName("BranchId").IsRequired();
            branch.Property(b => b.Name).HasColumnName("BranchName").IsRequired().HasMaxLength(100);
        });
        builder.Navigation(s => s.Branch).IsRequired();

        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(s => s.Status);

        builder.Property(s => s.TotalAmount).HasPrecision(18, 2);
        builder.Property(s => s.CreatedAt).IsRequired();

        builder.HasMany(s => s.Items)
            .WithOne()
            .HasForeignKey(i => i.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(s => s.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
