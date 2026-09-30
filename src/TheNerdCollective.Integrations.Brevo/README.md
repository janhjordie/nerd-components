# TheNerdCollective.Integrations.Brevo

.NET client for the Brevo API v3.

## Features

- Create or update a contact, including list membership
- Send a transactional email
- Dependency injection and Polly retry for transient failures

## Installation

```bash
dotnet add package TheNerdCollective.Integrations.Brevo
```

## Quick start

```json
{
  "Brevo": {
    "ApiKey": "your_brevo_api_key"
  }
}
```

```csharp
builder.Services.AddBrevoIntegration(builder.Configuration);
```

```csharp
await brevo.CreateOrUpdateContactAsync(
    "guest@example.com",
    attributes: new Dictionary<string, string> { ["FIRSTNAME"] = "Guest" },
    listIds: [12]);

await brevo.SendEmailAsync(
    senderEmail: "hello@example.com",
    senderName: "Example",
    toEmail: "inbox@example.com",
    toName: null,
    subject: "New inquiry",
    textContent: "A guest asked to be contacted.");
```

Attribute names must already exist in the Brevo account. The sender address must be a verified sender.
