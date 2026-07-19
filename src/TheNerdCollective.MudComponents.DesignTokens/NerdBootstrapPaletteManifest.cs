namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>Bootstrap 5 theme CSS variables for nerd intent bridge (HR-153).</summary>
public static class NerdBootstrapPaletteManifest
{
    public const string BootstrapVersion = "5.3";

    public static string BrandRootClass(string prefix) => $"{prefix}-bootstrap-brand";

    public static readonly string[] CoreTokens =
    [
        "bs-primary",
        "bs-secondary",
        "bs-success",
        "bs-info",
        "bs-warning",
        "bs-danger",
        "bs-body-bg",
        "bs-body-color",
        "bs-border-color",
        "bs-link-color"
    ];
}
