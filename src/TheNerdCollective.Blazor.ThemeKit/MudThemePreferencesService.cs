using Microsoft.Extensions.Options;

namespace TheNerdCollective.Blazor.ThemeKit;

public sealed class MudThemePreferencesService
{
    private readonly MudThemeKitOptions _options;

    public MudThemePreferencesService(IOptions<MudThemeKitOptions> options)
    {
        _options = options.Value;
    }

    public string ThemeIdKey => $"{_options.PreferencesStorageKeyPrefix}.themeId";

    public string IsDarkModeKey => $"{_options.PreferencesStorageKeyPrefix}.isDark";

    public string SavedThemesKey => $"{_options.PreferencesStorageKeyPrefix}.savedThemes";

    public string ThemeSessionsKey => $"{_options.PreferencesStorageKeyPrefix}.themeSessions";
}
