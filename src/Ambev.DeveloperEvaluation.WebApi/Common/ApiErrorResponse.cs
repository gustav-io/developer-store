namespace Ambev.DeveloperEvaluation.WebApi.Common;

/// <summary>
/// Error payload as defined in .doc/general-api.md.
/// </summary>
public class ApiErrorResponse
{
    /// <summary>Machine-readable error type identifier.</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Short, human-readable summary of the problem.</summary>
    public string Error { get; set; } = string.Empty;

    /// <summary>Human-readable explanation specific to this occurrence.</summary>
    public string Detail { get; set; } = string.Empty;
}
