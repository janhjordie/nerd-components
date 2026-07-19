using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>Maps semantic intents to Radzen theme CSS custom properties.</summary>
public sealed class NerdRadzenPaletteMap
{
    public required string Primary { get; init; }
    public required string Secondary { get; init; }
    public required string Success { get; init; }
    public required string Info { get; init; }
    public required string Warning { get; init; }
    public required string Danger { get; init; }
    public required string BodyBackground { get; init; }
    public required string TextColor { get; init; }
    public required string TextSecondaryColor { get; init; }

    public static NerdRadzenPaletteMap CreateConventionBindings() => new()
    {
        Primary = Intent(NerdDesignSystemUi.PrimaryAction, "surface"),
        Secondary = Intent(NerdDesignSystemUi.SecondaryAction, "surface"),
        Success = Intent(NerdDesignSystemUi.Success, "surface"),
        Info = Intent(NerdDesignSystemUi.Info, "surface"),
        Warning = Intent(NerdDesignSystemUi.Highlight, "surface"),
        Danger = Intent(NerdDesignSystemUi.Danger, "surface"),
        BodyBackground = Intent(NerdDesignSystemUi.PageSurface, "surface"),
        TextColor = Intent(NerdDesignSystemUi.PageSurface, "content"),
        TextSecondaryColor = Intent(NerdDesignSystemUi.MutedContent, "content")
    };

    private static string Intent(string alias, string channel) =>
        $"var({NerdIntentCssManifest.IntentVariable(alias, channel)})";
}
