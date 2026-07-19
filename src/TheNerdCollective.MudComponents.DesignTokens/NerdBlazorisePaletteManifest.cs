namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>Blazorise brand root + Bootstrap 5 variable bridge (HR-116).</summary>
public static class NerdBlazorisePaletteManifest
{
    public const string BlazoriseVersion = "1.7.3";

    public static string BrandRootClass(string prefix) => $"{prefix}-blazorise-brand";
}
