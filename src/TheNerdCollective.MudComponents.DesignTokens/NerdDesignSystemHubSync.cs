using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>Keeps <see cref="NerdDesignSystemOptions"/> in sync with the active token pack.</summary>
public static class NerdDesignSystemHubSync
{
    public static void FromTokenOptions(NerdDesignSystemOptions hub, NerdDesignTokenOptions tokens)
    {
        ArgumentNullException.ThrowIfNull(hub);
        ArgumentNullException.ThrowIfNull(tokens);

        hub.TokenPrefix = tokens.Prefix;
        hub.ActiveTokenPackId = tokens.ActiveBrandPackId;
        hub.ActiveBrandIdentityVersion = tokens.ActiveBrandIdentityVersion;
        hub.DesignTokenCount = tokens.Colors.Count;
        hub.DesignTokenRecipeCount = tokens.Recipes.Count;
        hub.NotifyBrandChanged();
    }
}
