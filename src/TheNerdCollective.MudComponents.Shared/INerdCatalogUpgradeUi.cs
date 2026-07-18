namespace TheNerdCollective.MudComponents.Shared;

/// <summary>Optional upgrade prompt component for gated catalog actions.</summary>
public interface INerdCatalogUpgradeUi
{
    /// <summary>Blazor component type for a custom upgrade prompt. Null = generic alert only.</summary>
    Type? UpgradePromptComponentType { get; }
}

/// <summary>Default — no branded upgrade UI in open-source hosts.</summary>
public sealed class NerdDefaultCatalogUpgradeUi : INerdCatalogUpgradeUi
{
    public Type? UpgradePromptComponentType => null;
}
