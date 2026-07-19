namespace TheNerdCollective.MudComponents.Shared;

/// <summary>
/// Host-provided access to active design-token and typography packs for bundle export/import.
/// </summary>
public interface INerdBrandPackSource
{
    bool IsConfigured { get; }

    string ClientId { get; }

    string ExportDesignTokensJson();

    string ExportTypographyJson();

    void ApplyDesignTokensJson(string json);

    void ApplyTypographyJson(string json);
}
