namespace Ambev.DeveloperEvaluation.Domain.ValueObjects;

/// <summary>
/// External identity of a branch with its denormalized name.
/// </summary>
public record BranchRef(Guid Id, string Name);
