using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

/// <summary>
/// Validator for ListSalesCommand.
/// </summary>
public class ListSalesValidator : AbstractValidator<ListSalesCommand>
{
    public const int MaxPageSize = 100;

    public ListSalesValidator()
    {
        RuleFor(c => c.Page).GreaterThanOrEqualTo(1);
        RuleFor(c => c.Size).InclusiveBetween(1, MaxPageSize);
        RuleFor(c => c.Order)
            .Must((command, order, context) =>
            {
                if (SaleOrderByParser.TryParse(order, out _, out var error))
                    return true;

                context.MessageFormatter.AppendArgument("Error", error);
                return false;
            })
            .WithMessage("{Error}");
        RuleFor(c => c.MaxDate)
            .GreaterThanOrEqualTo(c => c.MinDate)
            .When(c => c.MinDate.HasValue && c.MaxDate.HasValue)
            .WithMessage("_maxDate must be greater than or equal to _minDate");
        RuleFor(c => c.MaxTotalAmount)
            .GreaterThanOrEqualTo(c => c.MinTotalAmount)
            .When(c => c.MinTotalAmount.HasValue && c.MaxTotalAmount.HasValue)
            .WithMessage("_maxTotalAmount must be greater than or equal to _minTotalAmount");
    }
}
