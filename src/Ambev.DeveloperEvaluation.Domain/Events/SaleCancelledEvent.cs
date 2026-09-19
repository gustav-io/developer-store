using Ambev.DeveloperEvaluation.Domain.Entities;
using MediatR;

namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>Raised when a whole sale is cancelled.</summary>
public record SaleCancelledEvent(Sale Sale) : INotification;
