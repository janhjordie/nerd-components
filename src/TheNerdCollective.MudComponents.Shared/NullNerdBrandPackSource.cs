namespace TheNerdCollective.MudComponents.Shared;

public sealed class NullNerdBrandPackSource : INerdBrandPackSource
{
    public static readonly NullNerdBrandPackSource Instance = new();

    private NullNerdBrandPackSource()
    {
    }

    public bool IsConfigured => false;

    public string ClientId => "client";

    public string ExportDesignTokensJson() =>
        throw new InvalidOperationException("Brand pack source is not configured.");

    public string ExportTypographyJson() =>
        throw new InvalidOperationException("Brand pack source is not configured.");

    public void ApplyDesignTokensJson(string json) =>
        throw new InvalidOperationException("Brand pack source is not configured.");

    public void ApplyTypographyJson(string json) =>
        throw new InvalidOperationException("Brand pack source is not configured.");
}
