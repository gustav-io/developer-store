using Ambev.DeveloperEvaluation.Domain.Entities;
using MediatR;

namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>Raised when a sale header or its items are modified.</summary>
public record SaleModifiedEvent(Sale Sale) : INotification;
