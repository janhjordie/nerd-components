# TheNerdCollective.Blazor.ReCaptcha

Google **reCAPTCHA v2** (checkbox) for Blazor, with an automatic **simple math challenge** fallback when site/secret keys are not configured.

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
    options.SecretKey = "...";
});
```

`appsettings.json`:

```json
{
  "NerdReCaptcha": {
    "SiteKey": "",
    "SecretKey": ""
  }
}
```

When both keys are empty (or `Mode` is `Simple`), the component renders a local math challenge instead of Google.

## UI

```razor
@using TheNerdCollective.Blazor.ReCaptcha

<NerdReCaptcha @bind-Token="_token" />

@code {
    private string? _token;
}
```

## Server verify

```csharp
var result = await verifier.VerifyAsync(token);
if (!result.Success) { /* reject */ }
```

Simple tokens look like `simple:{challengeId}:{answer}` and are single-use.
