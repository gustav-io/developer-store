using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

/// <summary>
/// Validator for UpdateSaleCommand.
/// </summary>
public class UpdateSaleValidator : AbstractValidator<UpdateSaleCommand>
{
    public UpdateSaleValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.SaleDate).NotEqual(default(DateTime));
        RuleFor(c => c.CustomerId).NotEmpty();
        RuleFor(c => c.CustomerName).NotEmpty().MaximumLength(100);
        RuleFor(c => c.BranchId).NotEmpty();
        RuleFor(c => c.BranchName).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Items).NotEmpty().WithMessage("At least one item is required");
        RuleForEach(c => c.Items).SetValidator(new SaleItemCommandValidator());
    }
}
