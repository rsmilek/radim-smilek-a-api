using System.Net;
using Microsoft.Azure.Functions.Worker.Http;
using RadimSmilekAApi.Models;

namespace RadimSmilekAApi.Helpers;

/// <summary>Creates standardized HTTP responses for Azure Functions.</summary>
public static class ApiResponseWriter
{
    /// <summary>Creates a successful API response containing data.</summary>
    public static async Task<HttpResponseData> WriteSuccessAsync<T>(
        HttpRequestData request,
        HttpStatusCode statusCode,
        string message,
        T data,
        CancellationToken cancellationToken)
    {
        var response = request.CreateResponse(statusCode);
        await response.WriteAsJsonAsync(
            new ApiResponse<T> { Success = true, Message = message, Data = data },
            cancellationToken);
        return response;
    }

    /// <summary>Creates a failed API response containing optional validation errors.</summary>
    public static async Task<HttpResponseData> WriteErrorAsync(
        HttpRequestData request,
        HttpStatusCode statusCode,
        string message,
        CancellationToken cancellationToken,
        IReadOnlyList<string>? errors = null)
    {
        var response = request.CreateResponse(statusCode);
        await response.WriteAsJsonAsync(
            new ApiResponse<object?> { Success = false, Message = message, Errors = errors, Data = null },
            cancellationToken);
        return response;
    }
}