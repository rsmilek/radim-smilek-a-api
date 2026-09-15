namespace RadimSmilekAApi.Models;

/// <summary>Represents a successful or failed API operation.</summary>
/// <typeparam name="T">The type of data returned by the operation.</typeparam>
public sealed class ApiResponse<T>
{
    /// <summary>Indicates whether the operation completed successfully.</summary>
    public bool Success { get; set; }

    /// <summary>Optional human-readable message describing the result.</summary>
    public string? Message { get; set; }

    /// <summary>Optional error details returned when the operation fails.</summary>
    public IReadOnlyList<string>? Errors { get; set; }

    /// <summary>Optional data returned by a successful operation.</summary>
    public T? Data { get; set; }
}
