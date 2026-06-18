namespace TheNerdCollective.Blazor.ThemeKit;

public interface IThemeJsonFilePersistence
{
    bool IsAvailable { get; }

    Task<ThemeJsonFileSaveResult> SaveAsync(MudThemeJsonDocument document);

    Task<ThemeJsonFileCreateResult> CreateAsync(ThemeCreateRequest request);

    Task<ThemeJsonFileDeleteResult> DeleteAsync(string themeId);

    bool CanDeleteTheme(string themeId);
}

public sealed record ThemeCreateRequest(
    string Id,
    string DisplayName,
    string? CloneFromThemeId = null,
    string Version = "0.1.0");

public sealed record ThemeJsonFileSaveResult(
    bool Success,
    string? ThemeFilePath,
    string? IndexFilePath,
    string? Version,
    string? ErrorMessage);

public sealed record ThemeJsonFileCreateResult(
    bool Success,
    string? ThemeFilePath,
    string? ThemeId,
    string? ErrorMessage);

public sealed record ThemeJsonFileDeleteResult(
    bool Success,
    string? DeletedThemeId,
    string? FallbackThemeId,
    string? ErrorMessage);

public sealed class NullThemeJsonFilePersistence : IThemeJsonFilePersistence
{
    public bool IsAvailable => false;

    public Task<ThemeJsonFileSaveResult> SaveAsync(MudThemeJsonDocument document)
        => Task.FromResult(new ThemeJsonFileSaveResult(false, null, null, null, "JSON file persistence is not configured."));

    public Task<ThemeJsonFileCreateResult> CreateAsync(ThemeCreateRequest request)
        => Task.FromResult(new ThemeJsonFileCreateResult(false, null, null, "JSON file persistence is not configured."));

    public Task<ThemeJsonFileDeleteResult> DeleteAsync(string themeId)
        => Task.FromResult(new ThemeJsonFileDeleteResult(false, null, null, "JSON file persistence is not configured."));

    public bool CanDeleteTheme(string themeId) => false;
}
