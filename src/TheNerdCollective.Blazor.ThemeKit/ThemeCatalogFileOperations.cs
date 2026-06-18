using MudBlazor;

namespace TheNerdCollective.Blazor.ThemeKit;

public static class ThemeCatalogFileOperations
{
    public const string SharedUiSource = "shared-ui";
    public const string PlaybookLocalSource = "playbook-local";

    public static async Task<ThemeJsonFileCreateResult> CreateAsync(
        string themesDirectory,
        ThemeCreateRequest request,
        IMudThemeCatalog catalog,
        Action reloadCatalog)
    {
        if (!Directory.Exists(themesDirectory))
        {
            return new ThemeJsonFileCreateResult(false, null, null, "Themes directory not found.");
        }

        var themeId = request.Id.Trim();
        var displayName = request.DisplayName.Trim();

        if (!ThemeIdHelper.IsValidThemeId(themeId))
        {
            return new ThemeJsonFileCreateResult(
                false,
                null,
                null,
                "Theme id skal være lowercase med bindestreger, fx 'my-brand-theme'.");
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            return new ThemeJsonFileCreateResult(false, null, null, "Display name er påkrævet.");
        }

        try
        {
            var index = MudThemeJsonSerializer.LoadIndex(themesDirectory);
            if (index.Themes.Any(theme => theme.Id.Equals(themeId, StringComparison.OrdinalIgnoreCase)))
            {
                return new ThemeJsonFileCreateResult(false, null, null, $"Theme '{themeId}' findes allerede.");
            }

            var cloneFromId = ResolveCloneThemeId(request.CloneFromThemeId, index, catalog);
            var cloneTheme = catalog.GetTheme(cloneFromId);
            var updatedAt = ThemeVersionHelper.TodayIsoDate();
            var document = MudThemeJsonSerializer.CreateDocument(
                cloneTheme,
                themeId,
                request.Version,
                displayName,
                updatedAt);

            var fileName = $"{themeId}.theme.json";
            var themeFilePath = Path.Combine(themesDirectory, fileName);
            await File.WriteAllTextAsync(themeFilePath, MudThemeJsonSerializer.WriteThemeDocument(document));

            index.Themes.Add(new MudThemeIndexEntry
            {
                Id = themeId,
                DisplayName = displayName,
                Version = request.Version,
                UpdatedAt = updatedAt,
                PreviewPrimaryHex = document.PreviewPrimaryHex,
                File = fileName,
                Source = PlaybookLocalSource,
            });
            index.UpdatedAt = updatedAt;

            var indexFilePath = Path.Combine(themesDirectory, "themes.index.json");
            await File.WriteAllTextAsync(indexFilePath, MudThemeJsonSerializer.WriteIndexDocument(index));

            reloadCatalog();

            return new ThemeJsonFileCreateResult(true, themeFilePath, themeId, null);
        }
        catch (Exception ex)
        {
            return new ThemeJsonFileCreateResult(false, null, null, ex.Message);
        }
    }

    public static async Task<ThemeJsonFileDeleteResult> DeleteAsync(
        string themesDirectory,
        string themeId,
        Action reloadCatalog)
    {
        if (!Directory.Exists(themesDirectory))
        {
            return new ThemeJsonFileDeleteResult(false, null, null, "Themes directory not found.");
        }

        if (string.IsNullOrWhiteSpace(themeId))
        {
            return new ThemeJsonFileDeleteResult(false, null, null, "Theme id er påkrævet.");
        }

        try
        {
            var index = MudThemeJsonSerializer.LoadIndex(themesDirectory);
            var entry = index.Themes.FirstOrDefault(theme => theme.Id.Equals(themeId, StringComparison.OrdinalIgnoreCase));
            if (entry is null)
            {
                return new ThemeJsonFileDeleteResult(false, null, null, $"Theme '{themeId}' findes ikke i themes.index.json.");
            }

            if (IsProtectedTheme(entry))
            {
                return new ThemeJsonFileDeleteResult(
                    false,
                    null,
                    null,
                    $"Theme '{themeId}' er beskyttet (source: {entry.Source ?? SharedUiSource}) og kan ikke slettes.");
            }

            if (index.Themes.Count <= 1)
            {
                return new ThemeJsonFileDeleteResult(false, null, null, "Kan ikke slette det eneste theme i catalog.");
            }

            if (!string.IsNullOrWhiteSpace(entry.File))
            {
                var themeFilePath = Path.Combine(themesDirectory, entry.File);
                if (File.Exists(themeFilePath))
                {
                    File.Delete(themeFilePath);
                }
            }

            index.Themes.Remove(entry);
            index.UpdatedAt = ThemeVersionHelper.TodayIsoDate();

            if (index.DefaultThemeId.Equals(themeId, StringComparison.OrdinalIgnoreCase))
            {
                index.DefaultThemeId = ResolveFallbackThemeId(index);
            }

            var indexFilePath = Path.Combine(themesDirectory, "themes.index.json");
            await File.WriteAllTextAsync(indexFilePath, MudThemeJsonSerializer.WriteIndexDocument(index));

            reloadCatalog();

            return new ThemeJsonFileDeleteResult(true, themeId, index.DefaultThemeId, null);
        }
        catch (Exception ex)
        {
            return new ThemeJsonFileDeleteResult(false, null, null, ex.Message);
        }
    }

    public static bool IsProtectedTheme(MudThemeIndexEntry entry)
        => entry.Source?.Equals(SharedUiSource, StringComparison.OrdinalIgnoreCase) == true;

    public static bool CanDeleteTheme(string themeId, IReloadableMudThemeCatalog catalog)
    {
        var entry = catalog.TryGetIndexEntry(themeId);
        return entry is not null && !IsProtectedTheme(entry);
    }

    private static string ResolveCloneThemeId(
        string? requestedCloneId,
        MudThemeIndexDocument index,
        IMudThemeCatalog catalog)
    {
        if (!string.IsNullOrWhiteSpace(requestedCloneId)
            && index.Themes.Any(theme => theme.Id.Equals(requestedCloneId, StringComparison.OrdinalIgnoreCase)))
        {
            return requestedCloneId;
        }

        if (!string.IsNullOrWhiteSpace(index.DefaultThemeId)
            && index.Themes.Any(theme => theme.Id.Equals(index.DefaultThemeId, StringComparison.OrdinalIgnoreCase)))
        {
            return index.DefaultThemeId;
        }

        return catalog.DefaultThemeId;
    }

    private static string ResolveFallbackThemeId(MudThemeIndexDocument index)
    {
        var preferred = index.Themes.FirstOrDefault(theme =>
            theme.Id.Equals("billetsalg-default", StringComparison.OrdinalIgnoreCase));

        return preferred?.Id ?? index.Themes[0].Id;
    }
}
