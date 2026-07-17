namespace TheNerdCollective.Blazor.ThemeKit;

/// <summary>
/// Persists theme JSON files to a themes directory using <see cref="ThemeCatalogFileOperations"/>.
/// </summary>
public sealed class FileThemeJsonFilePersistence : IThemeJsonFilePersistence
{
    private readonly string _themesDirectory;
    private readonly IReloadableMudThemeCatalog _catalog;

    public FileThemeJsonFilePersistence(string themesDirectory, IReloadableMudThemeCatalog catalog)
    {
        _themesDirectory = themesDirectory;
        _catalog = catalog;
    }

    public bool IsAvailable => Directory.Exists(_themesDirectory);

    public async Task<ThemeJsonFileSaveResult> SaveAsync(MudThemeJsonDocument document)
    {
        if (!IsAvailable)
        {
            return new ThemeJsonFileSaveResult(false, null, null, null, "Themes directory not found.");
        }

        if (string.IsNullOrWhiteSpace(document.Id))
        {
            return new ThemeJsonFileSaveResult(false, null, null, null, "Theme id is required.");
        }

        try
        {
            var index = MudThemeJsonSerializer.LoadIndex(_themesDirectory);
            var entry = index.Themes.FirstOrDefault(theme =>
                theme.Id.Equals(document.Id, StringComparison.OrdinalIgnoreCase));
            if (entry is null)
            {
                return new ThemeJsonFileSaveResult(false, null, null, null, $"Theme '{document.Id}' was not found in themes.index.json.");
            }

            var fileName = string.IsNullOrWhiteSpace(entry.File)
                ? $"{document.Id}.theme.json"
                : entry.File;
            var themeFilePath = Path.Combine(_themesDirectory, fileName);
            await File.WriteAllTextAsync(themeFilePath, MudThemeJsonSerializer.WriteThemeDocument(document));

            entry.Version = document.Version;
            entry.UpdatedAt = document.UpdatedAt ?? ThemeVersionHelper.TodayIsoDate();
            entry.PreviewPrimaryHex = document.PreviewPrimaryHex ?? entry.PreviewPrimaryHex;
            index.UpdatedAt = entry.UpdatedAt;

            var indexFilePath = Path.Combine(_themesDirectory, "themes.index.json");
            await File.WriteAllTextAsync(indexFilePath, MudThemeJsonSerializer.WriteIndexDocument(index));
            _catalog.Reload();

            return new ThemeJsonFileSaveResult(true, themeFilePath, indexFilePath, document.Version, null);
        }
        catch (Exception ex)
        {
            return new ThemeJsonFileSaveResult(false, null, null, null, ex.Message);
        }
    }

    public Task<ThemeJsonFileCreateResult> CreateAsync(ThemeCreateRequest request) =>
        ThemeCatalogFileOperations.CreateAsync(_themesDirectory, request, _catalog, _catalog.Reload);

    public Task<ThemeJsonFileDeleteResult> DeleteAsync(string themeId) =>
        ThemeCatalogFileOperations.DeleteAsync(_themesDirectory, themeId, _catalog.Reload);

    public bool CanDeleteTheme(string themeId) =>
        ThemeCatalogFileOperations.CanDeleteTheme(themeId, _catalog);
}
