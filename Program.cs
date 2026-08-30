using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Communication.Email;
using Azure.Identity;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureOpenApi()
    .ConfigureServices((context, services) =>
    {
        services.Configure<JsonSerializerOptions>(options =>
        {
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });
        services.AddSingleton(_ => CreateEmailClient(context.Configuration));
    })
    .Build();

host.Run();

static EmailClient CreateEmailClient(IConfiguration configuration)
{
    var connectionString = configuration["ACS_CONNECTION_STRING"];
    if (!string.IsNullOrWhiteSpace(connectionString))
    {
        return new EmailClient(connectionString);
    }

    var endpoint = configuration["ACS_ENDPOINT"];
    if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var endpointUri) || endpointUri.Scheme != Uri.UriSchemeHttps)
    {
        throw new InvalidOperationException(
            "Configure ACS_ENDPOINT with an HTTPS Azure Communication Services endpoint, " +
            "or set ACS_CONNECTION_STRING for local development.");
    }

    return new EmailClient(endpointUri, new DefaultAzureCredential());
}