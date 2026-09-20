using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Services;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Services;

/// <summary>
/// Contains unit tests for the quantity-based discount tiers.
/// </summary>
public class DiscountPolicyTests
{
    [Theory(DisplayName = "Discount percent should follow the quantity tiers")]
    [InlineData(1, 0)]
    [InlineData(3, 0)]
    [InlineData(4, 0.10)]
    [InlineData(9, 0.10)]
    [InlineData(10, 0.20)]
    [InlineData(20, 0.20)]
    public void Given_Quantity_When_PercentFor_Then_ReturnsTierPercent(int quantity, decimal expected)
    {
        // Act
        var percent = DiscountPolicy.PercentFor(quantity);

        // Assert
        percent.Should().Be(expected);
    }

    [Theory(DisplayName = "Quantities outside 1..20 should be rejected")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(21)]
    public void Given_QuantityOutOfRange_When_PercentFor_Then_ThrowsDomainException(int quantity)
    {
        // Act
        var act = () => DiscountPolicy.PercentFor(quantity);

        // Assert
        act.Should().Throw<DomainException>();
    }
}
