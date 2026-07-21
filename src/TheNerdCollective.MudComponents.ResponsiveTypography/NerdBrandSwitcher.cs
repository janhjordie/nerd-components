using MudBlazor;
using TheNerdCollective.MudComponents.DesignTokens;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.ResponsiveTypography;

/// <summary>
/// Applies design-token + typography brand packs and notifies catalog pages.
/// </summary>
public sealed class NerdBrandSwitcher : INerdBrandSwitcher
{
    private readonly NerdDesignTokenOptions _tokenOptions;
    private readonly NerdDesignSystemOptions _hubOptions;
    private readonly NerdDesignTokenCss _tokenCss;
    private readonly INerdMudThemeController? _themeController;
    private readonly NerdResponsiveTypographyOptions? _typographyOptions;
    private readonly NerdResponsiveTypographyCss? _typographyCss;

    public NerdBrandSwitcher(
        NerdDesignTokenOptions tokenOptions,
        NerdDesignSystemOptions hubOptions,
        NerdDesignTokenCss tokenCss,
        INerdMudThemeController? themeController = null,
        NerdResponsiveTypographyOptions? typographyOptions = null,
        NerdResponsiveTypographyCss? typographyCss = null)
    {
        _tokenOptions = tokenOptions;
        _hubOptions = hubOptions;
        _tokenCss = tokenCss;
        _themeController = themeController;
        _typographyOptions = typographyOptions;
        _typographyCss = typographyCss;
    }

    public string ActiveBrandId =>
        _tokenOptions.ActiveBrandPackId ?? _tokenOptions.Prefix;

    public event Action<string>? BrandChanged;

    public void SwitchBrand(string brandId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(brandId);

        if (string.Equals(ActiveBrandId, brandId, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (_themeController is not null)
        {
            _themeController.ApplyBrandPack(brandId);
        }
        else
        {
            NerdBrandPackRegistry.Instance.Configure(brandId, _tokenOptions);
            foreach (var (id, set) in NerdThemeSetTools.CreateFromOptions(_tokenOptions))
            {
                if (!_tokenOptions.ThemeSets.ContainsKey(id))
                {
                    _tokenOptions.SetThemeSet(id, set);
                }
            }

            _tokenCss.Update(_tokenOptions);
            NerdDesignSystemHubSync.FromTokenOptions(_hubOptions, _tokenOptions);
        }

        ApplyBrandTypography(brandId);
        BrandChanged?.Invoke(brandId);
    }

    private void ApplyBrandTypography(string brandId)
    {
        if (_typographyOptions is null)
        {
            return;
        }

        NerdBrandTypographySwitcher.TrySwitchBrand(
            brandId,
            _typographyOptions,
            _hubOptions,
            _themeController?.CurrentTheme);
        _typographyCss?.Update(_typographyOptions.Typography);
    }
}
