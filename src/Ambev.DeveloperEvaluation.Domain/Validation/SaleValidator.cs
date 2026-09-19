using Ambev.DeveloperEvaluation.Domain.Entities;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.Domain.Validation;

/// <summary>
/// Structural validation of a Sale. Business invariants (quantities, duplicates, status) live in the aggregate.
/// </summary>
public class SaleValidator : AbstractValidator<Sale>
{
    public SaleValidator()
    {
        RuleFor(s => s.SaleDate).NotEqual(default(DateTime)).WithMessage("Sale date is required");
        RuleFor(s => s.Customer.Id).NotEmpty().WithMessage("Customer id is required");
        RuleFor(s => s.Customer.Name).NotEmpty().MaximumLength(100);
        RuleFor(s => s.Branch.Id).NotEmpty().WithMessage("Branch id is required");
        RuleFor(s => s.Branch.Name).NotEmpty().MaximumLength(100);
        RuleFor(s => s.Items).NotEmpty().WithMessage("A sale must have at least one item");
        RuleForEach(s => s.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Product.Id).NotEmpty();
            item.RuleFor(i => i.Product.Name).NotEmpty().MaximumLength(200);
            item.RuleFor(i => i.Product.UnitPrice).GreaterThan(0);
        });
    }
}
