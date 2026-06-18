namespace TheNerdCollective.Blazor.ThemeKit;

public sealed record MudThemeCatalogExportOptions(
    string Id,
    string DisplayName,
    string Version,
    string UpdatedAt,
    string? ClassName = null,
    string Namespace = "SharedUI.Themes.Catalog",
    string MudBlazorVersion = "9.5",
    string? SourceNotes = null);

public sealed record MudThemeProductionExport(
    string ThemeClassFile,
    string ThemeManifestFile,
    string Version,
    string UpdatedAt,
    string ClassName,
    string RelativeCsPath,
    string RelativeManifestPath);
