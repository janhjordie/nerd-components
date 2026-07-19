using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>Copy-paste snippets showing how semantic intents map per UI framework.</summary>
public static class NerdIntentUsageSnippets
{
    public static IReadOnlyList<NerdIntentUsageSnippet> CoreIntents { get; } =
    [
        Intent(NerdDesignSystemUi.PrimaryAction, "Filled CTA / primary button"),
        Intent(NerdDesignSystemUi.SecondaryAction, "Outlined / secondary button"),
        Intent(NerdDesignSystemUi.OnPrimaryAction, "Text on filled primary button"),
        Intent(NerdDesignSystemUi.PageSurface, "Main content background"),
        Intent(NerdDesignSystemUi.BrandChrome, "App bar / header chrome"),
        Intent(NerdDesignSystemUi.OnBrandChrome, "Text/icons on brand chrome"),
        Intent(NerdDesignSystemUi.NavSurface, "Drawer / side nav background"),
        Intent(NerdDesignSystemUi.NavItem, "Default nav link"),
        Intent(NerdDesignSystemUi.NavItemActive, "Active nav link accent"),
        Intent(NerdDesignSystemUi.InputSurface, "Text field / input surface"),
        Intent(NerdDesignSystemUi.InputBorder, "Input border"),
        Intent(NerdDesignSystemUi.FocusRing, "Focus ring accent"),
        Intent(NerdDesignSystemUi.MutedContent, "Secondary text / text buttons")
    ];

    private static NerdIntentUsageSnippet Intent(string alias, string usage) =>
        new(alias, usage,
            MudBlazor: $"Class=\"@Ui(\"{alias}\")\"",
            Radzen: $"class=\"rz-{alias}\" /* HR-115 */",
            FluentUi: $"class=\"fluent-{alias}\" /* planned */");

    public sealed record NerdIntentUsageSnippet(
        string IntentAlias,
        string Usage,
        string MudBlazor,
        string Radzen,
        string FluentUi);
}
