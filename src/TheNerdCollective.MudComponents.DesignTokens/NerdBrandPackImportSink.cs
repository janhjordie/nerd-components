using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>Applies imported token packs from the design system hub.</summary>
public sealed class NerdBrandPackImportSink : INerdBrandPackImportSink
{
    private readonly NerdDesignTokenOptions _options;
    private readonly NerdDesignTokenCss _tokenCss;
    private readonly NerdDesignSystemOptions _hubOptions;

    public NerdBrandPackImportSink(
        NerdDesignTokenOptions options,
        NerdDesignTokenCss tokenCss,
        NerdDesignSystemOptions hubOptions)
    {
        _options = options;
        _tokenCss = tokenCss;
        _hubOptions = hubOptions;
    }

    public Task<NerdBrandPackImportResult> ImportAsync(string json)
    {
        var result = NerdTokenPackImporter.TryImport(json);
        if (!result.Success || result.Pack is null)
        {
            return Task.FromResult(new NerdBrandPackImportResult(false, result.Message));
        }

        result.Pack.ApplyTo(_options);
        _tokenCss.Update(_options);
        NerdDesignSystemHubSync.FromTokenOptions(_hubOptions, _options);
        return Task.FromResult(new NerdBrandPackImportResult(true, result.Message));
    }
}
