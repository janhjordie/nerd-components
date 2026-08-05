# TheNerdCollective.MudComponents.FeatureFeedback

MudBlazor **feature ideas board** with suggest + upvote, optional reCAPTCHA, and injectable public/admin pages.

Depends on `TheNerdCollective.Blazor.FeatureFeedback` (+ `TheNerdCollective.Blazor.ReCaptcha` when captcha is registered).

## Install

```bash
dotnet add package TheNerdCollective.MudComponents.FeatureFeedback
```

## Host setup

```csharp
builder.Services.AddNerdReCaptcha(builder.Configuration); // optional — falls back to simple challenge
builder.Services.AddNerdFeatureFeedback<MyEfStore, MyAdminAccess>();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddNerdFeatureFeedbackPages(); // adds /feature-ideas + /admin/feature-ideas

// Also add the package assembly to <Router AdditionalAssemblies=...> in Routes.razor
```

Implement:

- `IFeatureFeedbackStore` — persistence (EF in your host)
- `IFeatureFeedbackAdminAccess` — who may open `/admin/feature-ideas`

Create + upvote require a non-empty `CurrentUserId` (signed-in user). Suggest form verifies reCAPTCHA when `INerdReCaptchaVerifier` is registered.
