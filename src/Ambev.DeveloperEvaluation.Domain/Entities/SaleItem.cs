using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Services;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// A line of a sale: a product reference, its quantity and the money computed by the discount policy.
/// Only the owning <see cref="Sale"/> creates or mutates items.
/// </summary>
public class SaleItem : BaseEntity
{
    /// <summary>Identifier of the owning sale.</summary>
    public Guid SaleId { get; private set; }

    /// <summary>External identity of the product with denormalized name and unit price.</summary>
    public ProductRef Product { get; private set; } = default!;

    /// <summary>Quantity sold. Between 1 and 20.</summary>
    public int Quantity { get; private set; }

    /// <summary>Discount percentage applied (0, 0.10 or 0.20).</summary>
    public decimal DiscountPercent { get; private set; }

    /// <summary>Discount amount in currency, rounded to 2 decimals.</summary>
    public decimal DiscountAmount { get; private set; }

    /// <summary>Line total after discount.</summary>
    public decimal TotalAmount { get; private set; }

    /// <summary>Whether this line was cancelled. Cancelled lines do not count towards the sale total.</summary>
    public bool IsCancelled { get; private set; }

    // Required by EF Core
    private SaleItem()
    {
    }

    internal SaleItem(Guid saleId, ProductRef product, int quantity)
    {
        if (product.UnitPrice <= 0)
            throw new DomainException("Unit price must be greater than zero");

        Id = Guid.NewGuid();
        SaleId = saleId;
        Product = product;
        Quantity = quantity;
        DiscountPercent = DiscountPolicy.PercentFor(quantity);

        var gross = product.UnitPrice * quantity;
        DiscountAmount = Math.Round(gross * DiscountPercent, 2, MidpointRounding.AwayFromZero);
        TotalAmount = gross - DiscountAmount;
    }

    internal void Cancel()
    {
        if (IsCancelled)
            throw new DomainException($"Item {Id} is already cancelled");

        IsCancelled = true;
    }
}
