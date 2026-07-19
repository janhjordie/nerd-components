using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>Standard component intents for workbook picker and documentation (HR-148).</summary>
public static class NerdIntentCatalogTools
{
    public static IReadOnlyList<NerdIntentCatalogEntry> StandardIntents { get; } =
    [
        new(NerdDesignSystemUi.PrimaryAction, "Filled buttons, primary CTAs", "MudButton Variant.Filled"),
        new(NerdDesignSystemUi.SecondaryAction, "Outlined / secondary actions", "MudButton Variant.Outlined"),
        new(NerdDesignSystemUi.OnPrimaryAction, "Text on primary-action surfaces", "MudButton label color"),
        new(NerdDesignSystemUi.PageSurface, "Page / card background", "MudPaper, main content"),
        new(NerdDesignSystemUi.BrandChrome, "App bar, chrome regions", "MudAppBar"),
        new(NerdDesignSystemUi.OnBrandChrome, "Text/icons on brand chrome", "MudAppBar title"),
        new(NerdDesignSystemUi.NavSurface, "Drawer / nav background", "MudDrawer"),
        new(NerdDesignSystemUi.NavItem, "Inactive nav links", "MudNavLink"),
        new(NerdDesignSystemUi.NavItemActive, "Active nav links", "MudNavLink active"),
        new(NerdDesignSystemUi.InputSurface, "Text field backgrounds", "MudTextField"),
        new(NerdDesignSystemUi.InputBorder, "Input borders", "MudTextField outlined"),
        new(NerdDesignSystemUi.FocusRing, "Focus ring color", "MudTextField focus"),
        new(NerdDesignSystemUi.MutedContent, "Secondary text", "MudText Typo.body2"),
        new(NerdDesignSystemUi.Info, "Info channel", "MudAlert Severity.Info"),
        new(NerdDesignSystemUi.Success, "Success channel", "MudAlert Severity.Success"),
        new(NerdDesignSystemUi.Danger, "Error / danger channel", "MudAlert Severity.Error"),
        new(NerdDesignSystemUi.Highlight, "Accent highlights", "MudChip")
    ];

    public static string? ResolveAliasTarget(NerdDesignTokenOptions options, string intentName) =>
        options.Aliases.TryGetValue(intentName, out var target) ? target : null;

    public static string FormatClass(NerdDesignTokenOptions options, string intentName) =>
        NerdDesignSystemUi.TokenClass(options.Prefix, intentName);
}

public sealed record NerdIntentCatalogEntry(string Name, string Summary, string UsageHint);
