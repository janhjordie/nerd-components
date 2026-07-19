using TheNerdCollective.MudComponents.DesignTokens;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.ResponsiveTypography;

internal sealed class NerdBrandPackSource : INerdBrandPackSource
{
    private readonly NerdDesignTokenOptions _tokenOptions;
    private readonly NerdResponsiveTypographyOptions _typographyOptions;
    private readonly NerdDesignTokenCss _tokenCss;
    private readonly NerdDesignSystemOptions _hubOptions;
    private string _clientId;

    public NerdBrandPackSource(
        NerdDesignTokenOptions tokenOptions,
        NerdResponsiveTypographyOptions typographyOptions,
        NerdDesignTokenCss tokenCss,
        NerdDesignSystemOptions hubOptions)
    {
        _tokenOptions = tokenOptions;
        _typographyOptions = typographyOptions;
        _tokenCss = tokenCss;
        _hubOptions = hubOptions;
        _clientId = hubOptions.ActiveTokenPackId
                    ?? hubOptions.ActiveTypographyPackId
                    ?? "client";
    }

    public bool IsConfigured => true;

    public string ClientId => _clientId;

    public string ExportDesignTokensJson() =>
        NerdTokenPack.FromOptions(_tokenOptions, _clientId).ToJson();

    public string ExportTypographyJson() =>
        NerdTypographyPack.FromOptions(_typographyOptions, _clientId).ToJson();

    public void ApplyDesignTokensJson(string json)
    {
        var pack = NerdTokenPack.FromJson(json);
        pack.ApplyTo(_tokenOptions);
        _tokenCss.Update(_tokenOptions);
        _hubOptions.ActiveTokenPackId = pack.ClientId;
        _hubOptions.DesignTokenCount = _tokenOptions.Colors.Count;
        _clientId = pack.ClientId;
    }

    public void ApplyTypographyJson(string json)
    {
        var pack = NerdTypographyPack.FromJson(json);
        pack.ApplyTo(_typographyOptions);
        _hubOptions.ActiveTypographyPackId = pack.ClientId;
        _clientId = pack.ClientId;
    }
}
