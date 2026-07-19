using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>Maps semantic intents to Bootstrap 5 CSS custom properties.</summary>
public sealed class NerdBootstrapPaletteMap
{
    public required string Primary { get; init; }
    public required string Secondary { get; init; }
    public required string Success { get; init; }
    public required string Info { get; init; }
    public required string Warning { get; init; }
    public required string Danger { get; init; }
    public required string BodyBackground { get; init; }
    public required string BodyColor { get; init; }
    public required string BorderColor { get; init; }
    public required string LinkColor { get; init; }

    public static NerdBootstrapPaletteMap CreateConventionBindings() => new()
    {
        Primary = Intent(NerdDesignSystemUi.PrimaryAction, "surface"),
        Secondary = Intent(NerdDesignSystemUi.SecondaryAction, "surface"),
        Success = Intent(NerdDesignSystemUi.Success, "surface"),
        Info = Intent(NerdDesignSystemUi.Info, "surface"),
        Warning = Intent(NerdDesignSystemUi.Highlight, "surface"),
        Danger = Intent(NerdDesignSystemUi.Danger, "surface"),
        BodyBackground = Intent(NerdDesignSystemUi.PageSurface, "surface"),
        BodyColor = Intent(NerdDesignSystemUi.PageSurface, "content"),
        BorderColor = Intent(NerdDesignSystemUi.InputBorder, "border"),
        LinkColor = Intent(NerdDesignSystemUi.PrimaryAction, "interactive")
    };

    private static string Intent(string alias, string channel) =>
        $"var({NerdIntentCssManifest.IntentVariable(alias, channel)})";
}
