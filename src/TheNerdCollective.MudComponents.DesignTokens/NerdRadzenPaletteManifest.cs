namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Radzen Blazor theme CSS variables used by the nerd intent bridge (HR-115).
/// </summary>
public static class NerdRadzenPaletteManifest
{
    public const string RadzenVersion = "5.7.0";

    public static string BrandRootClass(string prefix) => $"{prefix}-radzen-brand";

    public static readonly string[] CoreTokens =
    [
        "rz-primary",
        "rz-secondary",
        "rz-success",
        "rz-info",
        "rz-warning",
        "rz-danger",
        "rz-body-background-color",
        "rz-text-color",
        "rz-text-secondary-color"
    ];
}
