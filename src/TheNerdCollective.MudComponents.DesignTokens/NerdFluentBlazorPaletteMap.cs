using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>Maps semantic intents to Fluent UI Blazor CSS custom properties.</summary>
public sealed class NerdFluentBlazorPaletteMap
{
    public required string BrandBackground { get; init; }
    public required string BrandForeground { get; init; }
    public required string NeutralBackground { get; init; }
    public required string NeutralForeground { get; init; }
    public required string NeutralForegroundSecondary { get; init; }
    public required string NeutralStroke { get; init; }
    public required string FocusStroke { get; init; }

    public static NerdFluentBlazorPaletteMap CreateConventionBindings() => new()
    {
        BrandBackground = Intent(NerdDesignSystemUi.PrimaryAction, "surface"),
        BrandForeground = Intent(NerdDesignSystemUi.OnPrimaryAction, "content"),
        NeutralBackground = Intent(NerdDesignSystemUi.PageSurface, "surface"),
        NeutralForeground = Intent(NerdDesignSystemUi.PageSurface, "content"),
        NeutralForegroundSecondary = Intent(NerdDesignSystemUi.MutedContent, "content"),
        NeutralStroke = Intent(NerdDesignSystemUi.InputBorder, "border"),
        FocusStroke = Intent(NerdDesignSystemUi.FocusRing, "interactive")
    };

    private static string Intent(string alias, string channel) =>
        $"var({NerdIntentCssManifest.IntentVariable(alias, channel)})";
}
