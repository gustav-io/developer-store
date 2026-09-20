using Ambev.DeveloperEvaluation.Application.Sales.Common;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSale;

/// <summary>
/// Command for retrieving a sale by id.
/// </summary>
public record GetSaleCommand(Guid Id) : IRequest<SaleResult>;
