using System.Net;
using System.Net.Mail;
using System.Text.Json;
using Azure;
using Azure.Communication.Email;
using Azure.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using RadimSmilekAApi.Helpers;
using RadimSmilekAApi.Models;

namespace RadimSmilekAApi.Functions;

/// <summary>Sends email through Azure Communication Services.</summary>
public sealed class SendEmailFunction(
    ILogger<SendEmailFunction> logger,
    EmailClient emailClient)
{
    private const string SenderAddress = "DoNotReply@26fe5e8c-a166-4dac-960f-8d38c3267ca4.azurecomm.net";
    private const string BodyFormatPlainText = "plainText";
    private const string BodyFormatHtml = "html";

    /// <summary>
    /// http://localhost:7071/api/send-email
    /// Validates and submits an email for delivery through ACS - Azure Communication Services.
    /// </summary>
    /// <param name="request">HTTP request containing the email details.</param>
    /// <param name="cancellationToken">Signals that the invocation was cancelled.</param>
    /// <returns>An accepted response containing the ACS operation identifier.</returns>
    [Function("SendEmail")]
    [OpenApiOperation(
        operationId: "SendEmail",
        tags: ["Email"],
        Summary = "Send an email",
        Description = "Submits an email from rsw@rsw.one through Azure Communication Services.")]
    //[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
    [OpenApiSecurity(
        "function_key",
        SecuritySchemeType.ApiKey,
        Name = "code",
        In = OpenApiSecurityLocationType.Query,
        Description = "Azure Functions key supplied as the code query parameter or x-functions-key header.")]
    [OpenApiRequestBody(
        "application/json",
        typeof(SendEmailRequest),
        Required = true,
        Description = "Recipient, subject, body, optional reply-to address, and body format.")]
    [OpenApiResponseWithBody(HttpStatusCode.Accepted, "application/json", typeof(ApiResponse<string>), Description = "ACS accepted the send operation.")]
    [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof(ApiResponse<object>), Description = "The request is malformed or invalid.")]
    [OpenApiResponseWithBody(HttpStatusCode.BadGateway, "application/json", typeof(ApiResponse<object>), Description = "ACS rejected or failed the request.")]
    [OpenApiResponseWithBody(HttpStatusCode.InternalServerError, "application/json", typeof(ApiResponse<object>), Description = "Authentication or an unexpected error prevented submission.")]
    public async Task<HttpResponseData> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "sendemail")] HttpRequestData request,
        CancellationToken cancellationToken)
    {
        SendEmailRequest? payload;

        try
        {
            payload = await JsonSerializer.DeserializeAsync<SendEmailRequest>(request.Body, _serializeOptions, cancellationToken);
        }
        catch (JsonException)
        {
            return await ApiResponseWriter.WriteErrorAsync(request, HttpStatusCode.BadRequest, "Request body must be valid JSON.", cancellationToken);
        }

        var errors = Validate(payload);
        if (errors.Count > 0)
            return await ApiResponseWriter.WriteErrorAsync(request, HttpStatusCode.BadRequest, "Request validation failed.", cancellationToken, errors);

        var content = new EmailContent(payload!.Subject!);
        if (string.Equals(payload.BodyFormat, BodyFormatHtml, StringComparison.OrdinalIgnoreCase))
            content.Html = payload.Body;
        else
            content.PlainText = payload.Body;

        var message = new EmailMessage(SenderAddress, payload.To!, content);
        if (!string.IsNullOrWhiteSpace(payload.ReplyTo))
            message.ReplyTo.Add(new EmailAddress(payload.ReplyTo));

        try
        {
            var operation = await emailClient.SendAsync(WaitUntil.Started, message, cancellationToken);
            logger.LogInformation("ACS accepted email operation {OperationId} for recipient {Recipient}.", operation.Id, payload.To);
            return await ApiResponseWriter.WriteSuccessAsync(request, HttpStatusCode.Accepted, $"ACS accepted email operation for recipient {payload.To}.",
                operation.Id, cancellationToken);
        }
        catch (RequestFailedException exception)
        {
            logger.LogError(exception, "ACS failed to accept an email for recipient {Recipient}.", payload.To);
            return await ApiResponseWriter.WriteErrorAsync(request, HttpStatusCode.BadGateway, "The email provider could not accept the request.", cancellationToken);
        }
        catch (AuthenticationFailedException exception)
        {
            logger.LogError(exception, "ACS authentication failed.");
            return await ApiResponseWriter.WriteErrorAsync(request, HttpStatusCode.InternalServerError, "Email service authentication failed.", cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Unexpected error while submitting an email.");
            return await ApiResponseWriter.WriteErrorAsync(request, HttpStatusCode.InternalServerError, "An unexpected error prevented email submission.", cancellationToken);
        }
    }

    private static readonly JsonSerializerOptions _serializeOptions = new(JsonSerializerDefaults.Web);

    private static List<string> Validate(SendEmailRequest? payload)
    {
        var errors = new List<string>();

        if (payload is null)
        {
            errors.Add("Request body is required.");
            return errors;
        }

        ValidateRequiredEmail(payload.To, "to", errors);

        if (string.IsNullOrWhiteSpace(payload.Subject))
            errors.Add("Subject is required.");

        if (string.IsNullOrWhiteSpace(payload.Body))
            errors.Add("Body is required.");

        if (!string.IsNullOrWhiteSpace(payload.ReplyTo) && !MailAddress.TryCreate(payload.ReplyTo, out _))
            errors.Add("ReplyTo must be a valid email address.");

        if (!string.IsNullOrWhiteSpace(payload.BodyFormat) &&
            !string.Equals(payload.BodyFormat, BodyFormatPlainText, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(payload.BodyFormat, BodyFormatHtml, StringComparison.OrdinalIgnoreCase))
            errors.Add($"BodyFormat must be {BodyFormatPlainText} or {BodyFormatHtml}.");

        return errors;
    }

    private static void ValidateRequiredEmail(string? address, string fieldName, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(address))
            errors.Add($"{fieldName} is required.");
        else if (!MailAddress.TryCreate(address, out _))
            errors.Add($"{fieldName} must be a valid email address.");
    }

}