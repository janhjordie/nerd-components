# TheNerdCollective.Integrations.Umbraco.Brevo

Umbraco backoffice integration for Brevo:

- Top-level **Brevo** section
- Connection status dashboard
- Reads site settings from a published content node

Requires `TheNerdCollective.Integrations.Brevo` (API key via `Brevo:ApiKey`).

## Install

```bash
dotnet add package TheNerdCollective.Integrations.Umbraco.Brevo
```

Reference the package from your Umbraco site project. Composers are discovered automatically.

## Configuration

```json
{
  "Brevo": {
    "ApiKey": "your-key"
  },
  "UmbracoBrevo": {
    "ContentTypeAlias": "drykBrevoSettings",
    "SettingsContentKey": "f8e7d6c5-b4a3-4291-8e7d-6c5b4a392010",
    "SettingsLocationHint": "Content → Forside → Brevo"
  }
}
```

## Content type properties

The settings document type should expose (aliases):

- `newsletterListId` (integer)
- `senderEmail`, `senderName`, `notifyEmail` (text)

API keys must not be stored in Umbraco content.
