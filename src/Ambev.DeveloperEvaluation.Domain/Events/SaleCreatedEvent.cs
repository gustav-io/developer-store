using Ambev.DeveloperEvaluation.Domain.Entities;
using MediatR;

namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>Raised when a sale is created.</summary>
public record SaleCreatedEvent(Sale Sale) : INotification;
