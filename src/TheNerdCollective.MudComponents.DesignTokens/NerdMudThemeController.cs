using MudBlazor;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Default <see cref="INerdMudThemeController"/> for host + catalog brand switching (HR-172).
/// </summary>
public sealed class NerdMudThemeController : INerdMudThemeController
{
    private readonly NerdDesignTokenOptions _tokenOptions;
    private readonly NerdDesignTokenCss _tokenCss;
    private readonly NerdDesignSystemOptions _hubOptions;
    private readonly INerdMudThemeConfigurator _themeConfigurator;

    public NerdMudThemeController(
        NerdDesignTokenOptions tokenOptions,
        NerdDesignTokenCss tokenCss,
        NerdDesignSystemOptions hubOptions,
        INerdMudThemeConfigurator? themeConfigurator = null)
    {
        _tokenOptions = tokenOptions;
        _tokenCss = tokenCss;
        _hubOptions = hubOptions;
        _themeConfigurator = themeConfigurator ?? new NullNerdMudThemeConfigurator();
        CurrentTheme = NerdMudThemeFactory.Create(_tokenOptions, _themeConfigurator.Configure);
    }

    public MudTheme CurrentTheme { get; private set; }

    public bool IsDarkMode { get; private set; }

    public event Action? ThemeChanged;

    public void ApplyBrandPack(string brandPackId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(brandPackId);
        NerdBrandPackRegistry.Instance.Configure(brandPackId, _tokenOptions);
        _tokenCss.Update(_tokenOptions);
        NerdDesignSystemHubSync.FromTokenOptions(_hubOptions, _tokenOptions);
        RefreshTheme();
        _themeConfigurator.OnBrandPackApplied(brandPackId, CurrentTheme);
    }

    public void SetDarkMode(bool isDarkMode)
    {
        if (IsDarkMode == isDarkMode)
        {
            return;
        }

        IsDarkMode = isDarkMode;
        ThemeChanged?.Invoke();
    }

    public void RefreshTheme(Action<MudTheme>? configure = null)
    {
        CurrentTheme = NerdMudThemeFactory.Create(_tokenOptions, theme =>
        {
            if (configure is not null)
            {
                configure(theme);
            }
            else
            {
                _themeConfigurator.Configure(theme);
            }
        });
        ThemeChanged?.Invoke();
    }
}
