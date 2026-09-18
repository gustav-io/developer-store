using Ambev.DeveloperEvaluation.Domain.Exceptions;

namespace Ambev.DeveloperEvaluation.Domain.Services;

/// <summary>
/// Quantity-based discount tiers applied to identical items in a sale.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>1–3 items: no discount</item>
/// <item>4–9 items: 10%</item>
/// <item>10–20 items: 20%</item>
/// <item>more than 20 identical items cannot be sold</item>
/// </list>
/// </remarks>
public static class DiscountPolicy
{
    public const int MinQuantity = 1;
    public const int MaxQuantity = 20;
    public const int TenPercentMinQuantity = 4;
    public const int TwentyPercentMinQuantity = 10;

    /// <summary>
    /// Returns the discount percentage (0, 0.10 or 0.20) for a given quantity.
    /// </summary>
    /// <exception cref="DomainException">Quantity is below 1 or above 20.</exception>
    public static decimal PercentFor(int quantity)
    {
        if (quantity < MinQuantity)
            throw new DomainException($"Quantity must be at least {MinQuantity}");

        if (quantity > MaxQuantity)
            throw new DomainException($"Cannot sell more than {MaxQuantity} identical items");

        if (quantity >= TwentyPercentMinQuantity)
            return 0.20m;

        if (quantity >= TenPercentMinQuantity)
            return 0.10m;

        return 0m;
    }
}
