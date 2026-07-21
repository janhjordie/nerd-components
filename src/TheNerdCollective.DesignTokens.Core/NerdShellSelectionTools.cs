using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Shared shell selection accent (nav active, select selected, menu option) — 12% tint + content text.
/// </summary>
public static class NerdShellSelectionTools
{
    public const int AccentMixPercent = 12;

    public static string ResolveSelectedIntent(NerdDesignTokenOptions options)
    {
        var defaults = NerdTokenPackShellTools.ResolveMudBlazorDefaults(options);
        return defaults.Select?.Selected
               ?? defaults.NavLink?.Active
               ?? NerdDesignSystemUi.NavItemActive;
    }

    public static string ResolveAccentCssVariable(NerdDesignTokenOptions options)
    {
        var intent = ResolveSelectedIntent(options);
        return $"var(--{options.Prefix}-color-{intent})";
    }

    public static string ResolveContentCssVariable(NerdDesignTokenOptions options, string surfaceIntent)
    {
        return $"var(--{options.Prefix}-color-{surfaceIntent}-content)";
    }

    public static string BuildTintBackground(string accentCss, int mixPercent = AccentMixPercent) =>
        $"color-mix(in srgb, {accentCss} {mixPercent}%, transparent)";
}
