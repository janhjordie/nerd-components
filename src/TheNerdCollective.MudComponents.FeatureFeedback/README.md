# TheNerdCollective.MudComponents.FeatureFeedback

MudBlazor **feature ideas board** with suggest + upvote (PollUnit-style).

Depends on `TheNerdCollective.Blazor.FeatureFeedback`.

## Install

```bash
dotnet add package TheNerdCollective.MudComponents.FeatureFeedback
```

## Usage

```csharp
builder.Services.AddFeatureFeedbackStore<MyEfStore>();
// or AddInMemoryFeatureFeedback() for demos
```

```razor
@using TheNerdCollective.MudComponents.FeatureFeedback

<NerdFeatureIdeasBoard Title="Vi lytter til vores kunder"
                       Lead="Foreslå funktioner og stem på det, der betyder mest."
                       CurrentUserId="@_userId"
                       CurrentUserDisplayName="@_displayName"
                       LoginUrl="/account/login?ReturnUrl=%2Fideas"
                       OnRequireLogin="GoLogin" />
```

Unauthenticated users can browse ideas. Suggest and upvote require a non-empty `CurrentUserId`.
