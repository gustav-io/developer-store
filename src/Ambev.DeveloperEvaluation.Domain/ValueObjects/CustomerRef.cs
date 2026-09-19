namespace Ambev.DeveloperEvaluation.Domain.ValueObjects;

/// <summary>
/// External identity of a customer (owned by another bounded context) with its denormalized name.
/// </summary>
public record CustomerRef(Guid Id, string Name);
