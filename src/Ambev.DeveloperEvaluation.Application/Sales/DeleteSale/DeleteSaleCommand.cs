using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;

/// <summary>
/// Command for permanently deleting a sale.
/// </summary>
public record DeleteSaleCommand(Guid Id) : IRequest<DeleteSaleResponse>;
