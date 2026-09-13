using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;

namespace RadimSmilekAApi.Models;

/// <summary>Describes an email to submit through Azure Communication Services.</summary>
public sealed class SendEmailRequest
{
    /// <summary>Recipient email address.</summary>
    [Required]
    [OpenApiProperty(Nullable = false, Default = "rsw@rsw.one", Description = "Enter Email address")]
    public string? To { get; init; }

    /// <summary>Email subject.</summary>
    [Required]
    [OpenApiProperty(Nullable = false, Default = "Subject", Description = "Enter Email subject")]
    public string? Subject { get; init; }

    /// <summary>Plain-text or HTML email content.</summary>
    [Required]
    [OpenApiProperty(Nullable = true, Default = "Message", Description = "Enter Email message")]
    public string? Body { get; init; }

    /// <summary>Optional address that receives replies.</summary>
    [OpenApiProperty(Nullable = false, Default = "rsw@rsw.one", Description = "Enter Reply-To email address")]
    public string? ReplyTo { get; init; }

    /// <summary>Body format: <c>plainText</c> (default) or <c>html</c>.</summary>
    [OpenApiProperty(Nullable = false, Default = "plainText", Description = "Enter Body format: plainText or html")]
    public string? BodyFormat { get; init; }
}
