using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the _order query parameter parser.
/// </summary>
public class SaleOrderByParserTests
{
    [Fact(DisplayName = "Given multiple clauses When parsing Then returns fields and directions in order")]
    public void Parse_MultipleClauses_ReturnsOrderings()
    {
        // Act
        var result = SaleOrderByParser.Parse("\"totalAmount desc, saleDate asc, saleNumber\"");

        // Assert
        result.Should().HaveCount(3);
        result[0].Should().Be(new SaleOrdering("totalAmount", true));
        result[1].Should().Be(new SaleOrdering("saleDate", false));
        result[2].Should().Be(new SaleOrdering("saleNumber", false));
    }

    [Fact(DisplayName = "Given field in different casing When parsing Then normalizes to canonical name")]
    public void Parse_CaseInsensitive_Normalizes()
    {
        // Act
        var result = SaleOrderByParser.Parse("CUSTOMERNAME DESC");

        // Assert
        result.Should().ContainSingle().Which.Should().Be(new SaleOrdering("customerName", true));
    }

    [Theory(DisplayName = "Given empty order When parsing Then returns no orderings")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Parse_Empty_ReturnsEmpty(string? order)
    {
        SaleOrderByParser.Parse(order).Should().BeEmpty();
    }

    [Theory(DisplayName = "Given unknown field or direction When parsing Then fails with a message")]
    [InlineData("password desc")]
    [InlineData("saleDate sideways")]
    [InlineData("saleDate desc extra")]
    public void TryParse_Invalid_ReturnsFalse(string order)
    {
        // Act
        var ok = SaleOrderByParser.TryParse(order, out var orderings, out var error);

        // Assert
        ok.Should().BeFalse();
        orderings.Should().BeEmpty();
        error.Should().NotBeNullOrWhiteSpace();
    }
}
