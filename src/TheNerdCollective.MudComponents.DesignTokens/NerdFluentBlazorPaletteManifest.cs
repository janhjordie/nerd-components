namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Fluent UI Blazor v4 design-token names used by the nerd intent bridge (HR-117).
/// </summary>
public static class NerdFluentBlazorPaletteManifest
{
    public const string FluentUiVersion = "4.11.0";

    public static string BrandRootClass(string prefix) => $"{prefix}-fluent-brand";

    public static readonly string[] CoreTokens =
    [
        "colorBrandBackground",
        "colorBrandForeground1",
        "colorNeutralBackground1",
        "colorNeutralForeground1",
        "colorNeutralForeground2",
        "colorNeutralStroke1",
        "colorStrokeFocus1"
    ];
}
