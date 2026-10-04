# RadimSmilekAApi

## A. General Description

RadimSmilekAApi is a .NET 10 Azure Functions v4 application using the isolated worker model. It exposes a function-key-protected HTTP endpoint that submits email through Azure Communication Services (ACS) Email.

- Runtime: .NET 10, Azure Functions isolated worker
- Email provider: `Azure.Communication.Email`
- Authentication: managed identity through `DefaultAzureCredential`, with an optional local connection-string fallback
- API documentation: Azure Functions OpenAPI extension and Swagger UI
- Verified sender: `rsw@rsw.one`

The official `Microsoft.Azure.Functions.Worker.Extensions.OpenApi` package is used instead of literal NSwag middleware. NSwag's ASP.NET Core generator does not discover `[Function]` and `[HttpTrigger]` endpoints, while the Functions extension generates OpenAPI documents directly from Function metadata.

### Endpoint

`POST /api/sendemail`

The endpoint uses `AuthorizationLevel.Function`. Supply a function key through the `x-functions-key` header or the `code` query parameter.

Example request:

```http
POST /api/sendemail HTTP/1.1
Host: localhost:7071
Content-Type: application/json
x-functions-key: <function-key>

{
  "to": "rsw@rws.one",
  "subject": "New website contact",
  "body": "New message from the website contact form:\n\nName: Jan Novak\nEmail: customer@gmail.com\n\nMessage:\nHello, I am interested in...",
  "replyTo": "customer@gmail.com",
  "bodyFormat": "plainText"
}
```

`bodyFormat` is optional and accepts `plainText` or `html`. It defaults to `plainText`.

Successful submission returns `202 Accepted`:

```json
{
  "operationId": "<acs-operation-id>",
  "status": "Accepted"
}
```

Acceptance means ACS started the send operation. It does not guarantee final delivery.

### Configuration

| App setting | Required | Description |
| --- | --- | --- |
| `ACS_ENDPOINT` | Yes for managed identity | ACS endpoint such as `https://my-service.communication.azure.com`. Replace the placeholder before sending. |
| `ACS_CONNECTION_STRING` | Local fallback only | When nonempty, this takes precedence over `ACS_ENDPOINT`. Do not set it in Azure when managed identity is used. |
| `FUNCTIONS_WORKER_RUNTIME` | Yes | Must be `dotnet-isolated`. |
| `AzureWebJobsStorage` | Yes | Function host storage connection. |
| `Host.CORS` | Local development | Allowed browser origins for the local Functions host. The checked-in local configuration uses `*` to allow all origins during development; do not use this wildcard in production. |

The checked-in example is [`local.settings.json`](local.settings.json). Git ignores this file because it can contain credentials.

Before connecting a public website form to this API, add appropriate abuse controls such as CAPTCHA, origin restrictions, rate limiting, recipient allow-listing, and monitoring. A function key alone is not suitable for storage in browser code.

## B. OpenAPI and Swagger Debug Documentation

### Prerequisites

1. Install the .NET 10 SDK.
2. Install Azure Functions Core Tools v4.
3. Provide Function host storage by running Azurite for `UseDevelopmentStorage=true`, or replace `AzureWebJobsStorage` with a development storage connection string.
4. Choose one local ACS authentication method below.

For managed-identity-compatible local authentication, leave `ACS_CONNECTION_STRING` empty, set `ACS_ENDPOINT`, and authenticate a developer identity:

```powershell
az login
```

The signed-in identity needs permission to send email through the ACS resource.

For connection-string authentication, put a development ACS connection string in `ACS_CONNECTION_STRING`. Never commit that value.

### CORS for a frontend

CORS must allow the origin from which the browser loads the frontend, or the browser will block calls to this API. For local development, `local.settings.json` sets `Host.CORS` to `*`, allowing requests from any origin. This is for development convenience only.
```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
  },
  "Host": {
    "CORS": "*"
  }
}
```

In Azure, configure CORS on the Function App separately; local settings are not deployed. In the Azure portal, open the Function App, go to **API > CORS**, add the frontend's exact origin (for example, `https://www.example.com`), and save. Do not include a path or trailing slash. For multiple frontends, add each origin explicitly and avoid `*` in production. You can also configure an origin with Azure CLI:

```powershell
az functionapp cors add `
  --resource-group $resourceGroup `
  --name $functionApp `
  --allowed-origins https://www.example.com
```

CORS only controls which browser origins may make cross-origin requests; it does not replace the endpoint's function-key authentication.

### Run locally

```powershell
dotnet restore
func start
```

Once the Functions host is running, open:

- Swagger UI: <http://localhost:7071/api/swagger/ui>
- OpenAPI 3 document: <http://localhost:7071/api/openapi/v3.json>

Core Tools disables authorization by default for local development. Start it with `func start --enableAuth` when you need to test function-key enforcement. Azure always enforces the configured `AuthorizationLevel.Function`; supply the deployed key through Swagger's authorization control, the `x-functions-key` header, or the `code` query parameter.

## C. Azure Deployment Guide

### 1. Prepare Azure Communication Services Email

1. Create an Email Communication Services resource.
2. Under **Provision domains**, add `rsw.one` as a customer-managed domain.
3. Copy the ownership TXT record shown by Azure into the authoritative DNS zone for `rsw.one`, then start domain verification.
4. Add the exact SPF TXT record shown by Azure. Do not replace an existing SPF record blindly; a domain must have only one effective SPF policy, so merge required includes when necessary.
5. Add both DKIM CNAME records shown by Azure, normally using the generated `selector1-...._domainkey` and `selector2-...._domainkey` names.
6. Wait for domain, SPF, and both DKIM statuses to become verified.
7. Add or configure the sender username `rsw` so the complete MailFrom address is `rsw@rsw.one`.
8. Connect the verified Email Communication Services domain to the ACS resource used by this application.

Use values shown in the Azure portal for DNS records. Generated selector names and targets are resource-specific.

### 2. Create and configure the Function App

The app requires Azure Functions runtime v4, `dotnet-isolated`, .NET 10, 64-bit execution, and a supported hosting plan. Linux classic Consumption does not support .NET 10; use Flex Consumption, Premium, or Dedicated on Linux. Windows hosting can also be used where .NET 10 is offered.

Create the resource group, storage account, and Function App using the Azure portal, VS Code Azure Functions extension, or Azure CLI. Resource creation flags vary by hosting plan, so verify the current `az functionapp create` or `az functionapp flex-consumption create` options for the selected region.

After creation, enforce the worker settings:

```powershell
$resourceGroup = "<resource-group>"
$functionApp = "<globally-unique-function-app-name>"

az functionapp config appsettings set `
  --resource-group $resourceGroup `
  --name $functionApp `
  --settings FUNCTIONS_WORKER_RUNTIME=dotnet-isolated ACS_ENDPOINT=https://<acs-name>.communication.azure.com WEBSITE_USE_PLACEHOLDER_DOTNETISOLATED=1

az functionapp config set `
  --resource-group $resourceGroup `
  --name $functionApp `
  --net-framework-version v10.0 `
  --use-32bit-worker-process false
```

Keep `AzureWebJobsStorage` configured by the Function App provisioning process. Do not add `ACS_CONNECTION_STRING` when using managed identity.

### 3. Enable managed identity and ACS access

Enable the Function App's system-assigned identity:

```powershell
$principalId = az functionapp identity assign `
  --resource-group $resourceGroup `
  --name $functionApp `
  --query principalId `
  --output tsv

$acsId = az communication show `
  --resource-group $resourceGroup `
  --name "<acs-name>" `
  --query id `
  --output tsv
```

Assign the least-privilege email sender role exposed in your tenant, using the exact role name displayed by Azure:

```powershell
az role assignment create `
  --assignee-object-id $principalId `
  --assignee-principal-type ServicePrincipal `
  --role "Azure Communication Services Email Sender" `
  --scope $acsId
```

If that specialized role is unavailable, the prompt permits `Contributor` as a broader fallback:

```powershell
az role assignment create `
  --assignee-object-id $principalId `
  --assignee-principal-type ServicePrincipal `
  --role Contributor `
  --scope $acsId
```

Prefer the sender-specific role. RBAC propagation can take several minutes.

For a user-assigned identity, attach it to the Function App and set `AZURE_CLIENT_ID` to its client ID. `DefaultAzureCredential` then selects that identity.

### 4. Deploy the application

Build and publish locally first:

```powershell
dotnet publish --configuration Release --output .\publish
```

With Azure Functions Core Tools:

```powershell
func azure functionapp publish $functionApp --dotnet-isolated
```

With VS Code:

1. Install the Azure Functions extension and sign in.
2. Run **Azure Functions: Deploy to Function App**.
3. Select the subscription and existing Function App.
4. Confirm deployment, then inspect the Function App logs and routes.

For GitHub Actions, either download the workflow generated by the Function App Deployment Center or create a workflow that:

1. Checks out the repository.
2. Installs the .NET 10 SDK.
3. Runs `dotnet publish --configuration Release --output publish`.
4. Authenticates to Azure using OpenID Connect, preferably, or a protected publish-profile secret.
5. Deploys `publish` with `Azure/functions-action`.

Store Azure credentials only in GitHub environments/secrets. Do not place ACS credentials in the workflow; the deployed Function App uses managed identity.

### 5. Verify deployment

1. Open `https://<function-app>.azurewebsites.net/api/swagger/ui`.
2. Retrieve a host or function key from **Functions > App keys** in the Azure portal.
3. Submit a test payload using the key.
4. Confirm a `202` response and record the ACS operation ID.
5. Review Function App logs and ACS email logs for provider acceptance and final delivery status.