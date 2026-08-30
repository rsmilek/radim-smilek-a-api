namespace RadimSmilekAApi.Models;

/// <summary>Confirms that ACS accepted an email send operation.</summary>
public sealed class SendEmailResponse
{
    /// <summary>ACS operation identifier used for diagnostics.</summary>
    public required string OperationId { get; init; }

    /// <summary>Current submission state.</summary>
    public required string Status { get; init; }
}