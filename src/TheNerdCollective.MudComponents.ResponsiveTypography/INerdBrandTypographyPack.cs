namespace TheNerdCollective.MudComponents.ResponsiveTypography;

/// <summary>
/// Brand-specific responsive typography preset for catalog brand switching.
/// </summary>
public interface INerdBrandTypographyPack
{
    string Id { get; }

    void Configure(ResponsiveTypographyOptions options);
}
