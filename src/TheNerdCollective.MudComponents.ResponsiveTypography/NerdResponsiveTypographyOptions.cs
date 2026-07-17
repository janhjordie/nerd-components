namespace TheNerdCollective.MudComponents.ResponsiveTypography;

public sealed class NerdResponsiveTypographyOptions
{
    public ResponsiveTypographyOptions Typography { get; } = new();
    public bool EnableCatalogPage { get; set; } = true;
    public string CatalogRoute { get; set; } = "/nerd-typography";
    public bool RestrictCatalogToDevelopment { get; set; } = true;
    public bool WarnOnAccessibilityFailuresAtStartup { get; set; } = true;
    public string WcagVersion { get; set; } = NerdTypographyAccessibilityTools.DefaultWcagVersion;

    public MudBlazor.MudTheme CreatePreviewTheme()
    {
        var theme = new MudBlazor.MudTheme();
        theme.UseResponsiveTypography(Typography.CopyTo);
        return theme;
    }
}
