# Prompt: .NET 10 Azure Functions – ACS Email API

> **Usage**: Paste this prompt into ChatGPT, GitHub Copilot, Cursor, or any other AI coding assistant.
> **Suggested repo path**: `.github/prompts/azure-functions-acs-email-api.md`

---

You are a senior .NET developer. Create a **.NET 10 Azure Functions** project (isolated worker model) that exposes an HTTP endpoint for **sending emails via Azure Communication Services (ACS) – Email**.

---

## 📌 Project Requirements

### 1. General Requirements
- The project must be a **.NET 10 Azure Functions** project using the **isolated worker model**.
- Project name: `RadimSmilekAApi.csproj`
- The project must expose an HTTP endpoint for sending emails via **Azure Communication Services (ACS) – Email**.
- Email format example:
    From: azureemailservice@rws.one
    To: rsw@rws.one
    Reply-To: customer@gmail.com

    New message from the website contact form:

    Name: Jan Novák
    Email: customer@gmail.com

    Message:
    Hello, I am interested in...

### 1. Email Endpoint
- Create an HTTP-triggered Azure Function (e.g. `POST /api/send-email`) that accepts a JSON body with at minimum: `to`, `subject`, and `body` (HTML or plain text).
- The sender address must be: `rsw@rsw.one` (custom domain already configured in ACS).
- Use the **Azure Communication Services Email SDK** (`Azure.Communication.Email` NuGet package).

### 2. Configuration & Secrets
- All secrets and configuration values (ACS endpoint, connection string if needed) must be stored in **Azure Function App Settings** (environment variables), not hard-coded.
- **Preferred approach**: Use **Managed Identity** (System-assigned or User-assigned) to authenticate against ACS instead of connection strings. Show how to assign the role `Contributor` or `Azure Communication Services Email Sender` to the Managed Identity in ACS.
- Provide a local `local.settings.json` example with placeholder values for local development.

### 3. API Documentation with NSwag
- Integrate **NSwag** to auto-generate OpenAPI/Swagger documentation for the Azure Functions HTTP endpoints.
- The Swagger UI must be accessible at a local debug URL (e.g. `http://localhost:7071/api/swagger/ui`).
- Include all necessary NuGet packages and configuration in `Program.cs`.

### 4. README.md
Generate a `README.md` file with the following chapters:

**A. General Description**
Brief description of the project: purpose, tech stack (.NET 10, Azure Functions isolated worker, ACS Email, NSwag), and the sender domain `rsw.one`.

**B. NSwag Debug Documentation**
Link and instructions on how to access the Swagger UI during local development. Include the exact local URL and any prerequisites (e.g. running the Functions host).

**C. Azure Deployment Guide**
Step-by-step deployment instructions covering:
- Deploying the Azure Function App (using Azure CLI, VS Code, or GitHub Actions)
- Setting up **Azure Communication Services** with Email and custom domain `rsw.one` (DNS records, domain verification)
- Configuring **Managed Identity** on the Function App and assigning the correct ACS role/permissions
- Adding required App Settings in the Azure portal or via CLI

---

## ⚙️ Technical Constraints
- Target framework: **.NET 10**
- Azure Functions SDK: **isolated worker model** (`Microsoft.Azure.Functions.Worker`)
- Authentication to ACS: prefer `DefaultAzureCredential` (Managed Identity compatible)
- NSwag package: `Microsoft.Azure.WebJobs.Extensions.OpenApi` or equivalent compatible with isolated model
- Code must be clean, follow best practices, and include XML doc comments on the endpoint for NSwag to pick up

---

Generate the full project structure including: `Program.cs`, the Function class, `local.settings.json`, `.csproj` with all NuGet references, and `README.md`.
