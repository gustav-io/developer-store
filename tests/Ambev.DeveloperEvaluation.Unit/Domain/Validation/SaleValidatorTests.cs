using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Validation;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentValidation.TestHelper;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Validation;

/// <summary>
/// Contains unit tests for the SaleValidator class.
/// </summary>
public class SaleValidatorTests
{
    private readonly SaleValidator _validator = new();

    [Fact(DisplayName = "Valid sale should pass validation")]
    public void Given_ValidSale_When_Validated_Then_ShouldNotHaveErrors()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale(2);

        // Act
        var result = _validator.TestValidate(sale);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = "Sale with blank customer or branch name should fail validation")]
    public void Given_BlankNames_When_Validated_Then_ShouldHaveErrors()
    {
        // Arrange
        var sale = Sale.Create(
            DateTime.UtcNow,
            new CustomerRef(Guid.NewGuid(), ""),
            new BranchRef(Guid.NewGuid(), " "),
            new[] { SaleTestData.GenerateItem(1) });

        // Act
        var result = _validator.TestValidate(sale);

        // Assert
        result.ShouldHaveValidationErrorFor(s => s.Customer.Name);
        result.ShouldHaveValidationErrorFor(s => s.Branch.Name);
    }
}
