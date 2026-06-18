using MudBlazor;

namespace TheNerdCollective.Blazor.ThemeKit;

public interface IMudThemeStateService
{
    event Action? Changed;

    bool IsInitialized { get; }

    MudTheme CurrentTheme { get; }

    string CurrentThemeId { get; }

    bool IsDarkMode { get; }

    bool HasUnsavedChanges { get; }

    bool HasSavedSession { get; }

    void ReloadTheme();

    Task InitializeAsync();

    void SetTheme(string themeId);

    void SetDarkMode(bool isDarkMode);

    void SetToken(string tokenId, string value);

    string? GetToken(string tokenId);

    string? GetCatalogToken(string tokenId);

    bool IsTokenModified(string tokenId);

    IReadOnlyList<ThemeTokenChange> GetModifiedTokens();

    void ResetToken(string tokenId);

    MudThemeDescriptor? GetCurrentDescriptor();

    string GetSuggestedNextVersion();

    string ExportCatalogThemeClass(string? version = null);

    string ExportThemeManifest(string? version = null, string? mudBlazorVersion = null);

    MudThemeProductionExport ExportProductionTheme(string? version = null);

    string ExportJson(string? version = null, string? updatedAt = null);

    MudThemeJsonDocument CreateExportDocument(string? version = null, string? updatedAt = null);

    IReadOnlyList<MudThemeSessionSummary> GetSavedSessions();

    Task SaveSessionAsync();

    Task ClearSessionAsync();

    bool TryImportThemeJson(string json, out string? errorMessage);
}
