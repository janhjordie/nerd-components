using MudBlazor;

namespace TheNerdCollective.Blazor.ThemeKit;

/// <summary>
/// JSON-backed catalog that can be reloaded after theme files change on disk.
/// </summary>
public interface IReloadableMudThemeCatalog : IMudThemeCatalog
{
    void Reload();

    MudThemeIndexEntry? TryGetIndexEntry(string themeId);
}
