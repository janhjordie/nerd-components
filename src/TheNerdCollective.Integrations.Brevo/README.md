# TheNerdCollective.Integrations.Brevo

.NET client for the Brevo API v3.

## Features

- Create or update a contact with typed, generic attributes and list membership
- Read, create, update supported options, and delete contact attributes
- Send a transactional email
- Resolve API keys from configuration or an optional persistent store
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
await brevo.CreateOrUpdateContactWithAttributesAsync(
    "guest@example.com",
    attributes: new Dictionary<string, object?>
    {
        ["FIRSTNAME"] = "Guest",
        ["INTERESTS"] = new[] { "cafe", "news" },
        ["INQUIRY_TYPE"] = new[] { "tasting_box" }
    },
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
The original `CreateOrUpdateContactAsync` and `CreateOrUpdateContactAndGetIdAsync` methods remain available for string-valued attributes. Use the `...WithAttributes...` methods for arrays, booleans, numbers, and other typed values.

## Manage contact attributes

```csharp
BrevoDataResult<IReadOnlyList<BrevoContactAttribute>> attributes =
    await brevo.ListContactAttributesAsync();

BrevoResult create = await brevo.CreateContactAttributeAsync(
    "normal",
    "INTERESTS",
    new BrevoContactAttributeWriteOptions(
        Type: "multiple-choice",
        MultiCategoryOptions: ["cafe", "products", "recipes", "news"]));

BrevoResult update = await brevo.UpdateContactAttributeAsync(
    "normal",
    "INTERESTS",
    new BrevoContactAttributeWriteOptions(
        MultiCategoryOptions: ["cafe", "products", "recipes", "news"]));

BrevoResult delete = await brevo.DeleteContactAttributeAsync("normal", "INTERESTS");
```

`GetContactAttributeAsync` reads one attribute from Brevo's attribute catalog. Brevo's update endpoint can update supported enumeration values, multiple-choice options, or calculated/global formulas, but cannot change an attribute's type. Deleting an attribute permanently removes its values from contacts.
