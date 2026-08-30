using System.ComponentModel.DataAnnotations;

namespace RadimSmilekAApi.Models;

/// <summary>Describes an email to submit through Azure Communication Services.</summary>
public sealed class SendEmailRequest
{
    /// <summary>Recipient email address.</summary>
    [Required]
    public string? To { get; init; }

    /// <summary>Email subject.</summary>
    [Required]
    public string? Subject { get; init; }

    /// <summary>Plain-text or HTML email content.</summary>
    [Required]
    public string? Body { get; init; }

    /// <summary>Optional address that receives replies.</summary>
    public string? ReplyTo { get; init; }

    /// <summary>Body format: <c>plainText</c> (default) or <c>html</c>.</summary>
    public string? BodyFormat { get; init; }
}