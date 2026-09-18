using Ambev.DeveloperEvaluation.Domain.Entities;
using MediatR;

namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>Raised when a single item of a sale is cancelled.</summary>
public record ItemCancelledEvent(Sale Sale, SaleItem Item) : INotification;
