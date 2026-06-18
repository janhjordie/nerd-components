using MudBlazor;

namespace TheNerdCollective.Blazor.ThemeKit;

public interface IMudThemeCatalog
{
    string DefaultThemeId { get; }

    IReadOnlyList<MudThemeDescriptor> All { get; }

    MudTheme GetTheme(string themeId);
}
