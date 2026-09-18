using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

/// <summary>
/// Validator for CreateSaleCommand. Validates shape; business rules are enforced by the Sale aggregate.
/// </summary>
public class CreateSaleValidator : AbstractValidator<CreateSaleCommand>
{
    public CreateSaleValidator()
    {
        RuleFor(c => c.CustomerId).NotEmpty();
        RuleFor(c => c.CustomerName).NotEmpty().MaximumLength(100);
        RuleFor(c => c.BranchId).NotEmpty();
        RuleFor(c => c.BranchName).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Items).NotEmpty().WithMessage("At least one item is required");
        RuleForEach(c => c.Items).SetValidator(new SaleItemCommandValidator());
    }
}

/// <summary>
/// Validator for a sale item line.
/// </summary>
public class SaleItemCommandValidator : AbstractValidator<SaleItemCommand>
{
    public SaleItemCommandValidator()
    {
        RuleFor(i => i.ProductId).NotEmpty();
        RuleFor(i => i.ProductName).NotEmpty().MaximumLength(200);
        RuleFor(i => i.UnitPrice).GreaterThan(0);
        RuleFor(i => i.Quantity).GreaterThan(0);
    }
}
