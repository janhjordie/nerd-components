# TheNerdCollective.Blazor.FeatureFeedback

Host-agnostic **feature ideas / upvote** abstractions (PollUnit-style “collect ideas”).

Pair with `TheNerdCollective.MudComponents.FeatureFeedback` for the MudBlazor board UI.

## Install

```bash
dotnet add package TheNerdCollective.Blazor.FeatureFeedback
```

## Contracts

Implement `IFeatureFeedbackStore` in your host (EF Core, Cosmos, etc.), or use the in-memory store for demos:

```csharp
builder.Services.AddInMemoryFeatureFeedback();
// production:
builder.Services.AddFeatureFeedbackStore<ConsentFeatureFeedbackStore>();
```

Models:

- `FeatureIdeaDto`
- `CreateFeatureIdeaRequest`
- `FeatureIdeaMutationResult`
- `FeatureIdeaSort` / `FeatureIdeaStatus`

Hosts should enforce authentication before `CreateAsync` / `ToggleVoteAsync` (the store also returns `unauthenticated` when `userId` is empty).
