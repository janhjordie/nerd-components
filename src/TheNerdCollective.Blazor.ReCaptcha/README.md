# TheNerdCollective.Blazor.ReCaptcha

Google **reCAPTCHA v2** (checkbox) and **reCAPTCHA Enterprise** (score-based) for Blazor, with an automatic **simple math challenge** fallback when Google configuration is not provided.

## Install

```bash
dotnet add package TheNerdCollective.Blazor.ReCaptcha
```

## Setup

```csharp
builder.Services.AddNerdReCaptcha(builder.Configuration);
// or
builder.Services.AddNerdReCaptcha(options =>
{
    options.SiteKey = "...";
    options.ProjectId = "...";
    options.ApiKey = "...";
    options.Action = "CONTACT";
});
```

`appsettings.json`:

```json
{
  "NerdReCaptcha": {
    "Mode": "GoogleEnterprise",
    "SiteKey": "",
    "ProjectId": "",
    "ApiKey": "",
    "Action": "CONTACT",
    "MinimumScore": 0.5
  }
}
```

Enterprise mode loads `enterprise.js`, executes the `Action` configured in `NerdReCaptchaOptions` when the containing form is submitted, and verifies the token by creating an assessment through the reCAPTCHA Enterprise API. The server accepts only valid tokens with the configured action and a score at or above `MinimumScore`. Configure the action in `NerdReCaptchaOptions`; it is deliberately not overridable per component so the browser cannot choose the action the server trusts.

Restrict the Google Cloud API key to the reCAPTCHA Enterprise API and keep it server-side. Configure `SiteKey`, `ProjectId`, and `ApiKey`; do not expose `ApiKey` to the browser.

For the legacy v2 checkbox, configure `SiteKey` and `SecretKey` with `Mode` set to `GoogleV2`. When Google configuration is missing (or `Mode` is `Simple`), the component renders a local math challenge instead.

## UI

```razor
@using TheNerdCollective.Blazor.ReCaptcha

<form method="post">
    <NerdReCaptcha @bind-Token="_token" />
    <button type="submit">Send</button>
</form>

@code {
    private string? _token;
}
```

For button-driven submissions without a form submit event, call `GenerateTokenAsync()` on an `@ref` to the component immediately before sending the request. The method returns a fresh Enterprise token and publishes it through `TokenChanged`.

## Server verify

```csharp
var result = await verifier.VerifyAsync(token);
if (!result.Success) { /* reject */ }
```

The component includes a hidden `captchaToken` field for native form submission. Set `TokenInputName` if your server expects a different field name. Existing implementations of `INerdReCaptchaVerifier` can omit the Enterprise-only `Action` member and rely on the default implementation. Simple tokens look like `simple:{challengeId}:{answer}` and are single-use.
