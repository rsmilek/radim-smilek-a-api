namespace RadimSmilekAApi.Models;

/// <summary>Represents a client-safe API error.</summary>
public sealed class ApiErrorResponse
{
    /// <summary>Stable error summary.</summary>
    public required string Message { get; init; }

    /// <summary>Optional validation details.</summary>
    public IReadOnlyList<string>? Errors { get; init; }
}