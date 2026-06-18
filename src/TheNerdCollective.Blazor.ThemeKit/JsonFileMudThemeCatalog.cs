using MudBlazor;

namespace TheNerdCollective.Blazor.ThemeKit;

/// <summary>
/// Loads themes from a <c>themes.index.json</c> plus per-theme JSON files in a directory.
/// Falls back to an inner catalog when the directory or index is missing.
/// </summary>
public sealed class JsonFileMudThemeCatalog : IReloadableMudThemeCatalog
{
    private readonly string _themesDirectory;
    private readonly IMudThemeCatalog _fallbackCatalog;
    private readonly Dictionary<string, MudTheme> _themes = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, MudThemeIndexEntry> _indexEntries = new(StringComparer.OrdinalIgnoreCase);

    public JsonFileMudThemeCatalog(string themesDirectory, IMudThemeCatalog fallbackCatalog)
    {
        _themesDirectory = themesDirectory;
        _fallbackCatalog = fallbackCatalog;
        DefaultThemeId = fallbackCatalog.DefaultThemeId;
        All = fallbackCatalog.All;
        Reload();
    }

    public string DefaultThemeId { get; private set; }

    public IReadOnlyList<MudThemeDescriptor> All { get; private set; }

    public MudTheme GetTheme(string themeId)
    {
        if (_themes.TryGetValue(themeId, out var theme))
        {
            return MudThemeCloner.Clone(theme);
        }

        return _fallbackCatalog.GetTheme(themeId);
    }

    public MudThemeIndexEntry? TryGetIndexEntry(string themeId)
        => _indexEntries.TryGetValue(themeId, out var entry) ? entry : null;

    public void Reload()
    {
        _themes.Clear();
        _indexEntries.Clear();
        DefaultThemeId = _fallbackCatalog.DefaultThemeId;
        All = _fallbackCatalog.All;

        if (!Directory.Exists(_themesDirectory))
        {
            return;
        }

        try
        {
            var index = MudThemeJsonSerializer.LoadIndex(_themesDirectory);
            if (!string.IsNullOrWhiteSpace(index.DefaultThemeId))
            {
                DefaultThemeId = index.DefaultThemeId;
            }

            var descriptors = new List<MudThemeDescriptor>();
            foreach (var entry in index.Themes)
            {
                if (string.IsNullOrWhiteSpace(entry.Id))
                {
                    continue;
                }

                _indexEntries[entry.Id] = entry;
                _themes[entry.Id] = LoadThemeEntry(entry);
                descriptors.Add(new MudThemeDescriptor(
                    entry.Id,
                    entry.DisplayName,
                    entry.Version,
                    entry.PreviewPrimaryHex,
                    entry.UpdatedAt,
                    entry.Source ?? "json"));
            }

            if (descriptors.Count > 0)
            {
                All = descriptors;
            }
        }
        catch (FileNotFoundException)
        {
            // Keep fallback catalog metadata.
        }
    }

    private MudTheme LoadThemeEntry(MudThemeIndexEntry entry)
    {
        var baseTheme = _fallbackCatalog.GetTheme(entry.Id);

        if (string.IsNullOrWhiteSpace(entry.File))
        {
            return MudThemeCloner.Clone(baseTheme);
        }

        try
        {
            var document = MudThemeJsonSerializer.LoadThemeFile(_themesDirectory, entry.File);
            return MudThemeJsonSerializer.ApplyDocument(baseTheme, document);
        }
        catch (FileNotFoundException)
        {
            return MudThemeCloner.Clone(baseTheme);
        }
    }
}
