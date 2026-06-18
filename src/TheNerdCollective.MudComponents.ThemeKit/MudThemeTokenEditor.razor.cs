using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazor.Utilities;
using TheNerdCollective.Blazor.ThemeKit;

namespace TheNerdCollective.MudComponents.ThemeKit;

public partial class MudThemeTokenEditor : IDisposable
{
    private static readonly DialogOptions _dialogOptions = new() { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
    private static readonly DialogOptions _wideDialogOptions = new() { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Medium, FullWidth = true };

    private string _search = string.Empty;
    private string _importJson = string.Empty;
    private string _nextVersion = string.Empty;
    private bool _showOnlyModified;
    private bool _saveFileDialogOpen;
    private bool _importDialogOpen;
    private bool _createThemeDialogOpen;
    private bool _deleteThemeDialogOpen;
    private string _newThemeId = string.Empty;
    private string _newThemeDisplayName = string.Empty;
    private string _newThemeCloneFromId = string.Empty;
    private bool _newThemeIdAutoSuggested = true;
    private MudThemeDescriptor? _descriptor;
    private int _modifiedCount;
    private readonly Dictionary<string, bool> _expandedGroups = new(StringComparer.OrdinalIgnoreCase);

    [Inject]
    private IMudThemeStateService ThemeState { get; set; } = null!;

    [Inject]
    private IMudThemeCatalog Catalog { get; set; } = null!;

    [Inject]
    private IThemeJsonFilePersistence FilePersistence { get; set; } = null!;

    [Inject]
    private IMudThemeSessionStore SessionStore { get; set; } = null!;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    protected override void OnInitialized()
    {
        ThemeState.Changed += HandleThemeChanged;
        SyncExpandedGroups();
        RefreshDescriptor();
    }

    private void HandleThemeChanged()
    {
        SyncExpandedGroups();
        RefreshDescriptor();
        InvokeAsync(StateHasChanged);
    }

    private void RefreshDescriptor()
    {
        _descriptor = ThemeState.GetCurrentDescriptor();
        _modifiedCount = ThemeState.GetModifiedTokens().Count;
    }

    private async Task DownloadJsonExportAsync()
    {
        var export = ThemeState.ExportJson();
        var fileName = GetThemeFileName();
        try
        {
            await JsRuntime.InvokeVoidAsync("themeKit.downloadText", fileName, export);
            Snackbar.Add($"Downloaded {fileName}", Severity.Success);
        }
        catch (JSException)
        {
            await CopyJsonExportAsync();
        }
    }

    private async Task OnImportFileSelectedAsync(InputFileChangeEventArgs args)
    {
        var file = args.File;
        if (file is null)
        {
            return;
        }

        await using var stream = file.OpenReadStream(maxAllowedSize: 512_000);
        using var reader = new StreamReader(stream);
        _importJson = await reader.ReadToEndAsync();
        StateHasChanged();
    }

    private string GetGroupTitle(ThemeTokenEditorGroup group)
    {
        var count = GetGroupModifiedCount(group);
        return count > 0 ? $"{group.Title} ({count})" : group.Title;
    }

    private int GetGroupModifiedCount(ThemeTokenEditorGroup group)
        => group.Sections
            .SelectMany(section => section.Rows)
            .SelectMany(row => row.TokenIds)
            .Count(tokenId => ThemeState.IsTokenModified(tokenId));

    private async Task SaveSessionAsync()
    {
        await ThemeState.SaveSessionAsync();
        Snackbar.Add($"Session gemt for {ThemeState.CurrentThemeId}", Severity.Success);
    }

    private async Task DiscardSessionAsync()
    {
        await ThemeState.ClearSessionAsync();
        Snackbar.Add($"Session kasseret — catalog-version indlæst", Severity.Info);
    }

    private void OpenSaveFileDialog()
    {
        _nextVersion = ThemeState.GetSuggestedNextVersion();
        _saveFileDialogOpen = true;
    }

    private void CloseSaveFileDialog() => _saveFileDialogOpen = false;

    private async Task SaveThemeFileAsync()
    {
        if (string.IsNullOrWhiteSpace(_nextVersion))
        {
            return;
        }

        var document = ThemeState.CreateExportDocument(_nextVersion.Trim(), ThemeVersionHelper.TodayIsoDate());
        var result = await FilePersistence.SaveAsync(document);
        if (!result.Success)
        {
            Snackbar.Add(result.ErrorMessage ?? "Kunne ikke gemme JSON-fil", Severity.Error);
            return;
        }

        await ThemeState.ClearSessionAsync();
        _saveFileDialogOpen = false;
        RefreshDescriptor();

        Snackbar.Add($"Gemt v{result.Version} → {Path.GetFileName(result.ThemeFilePath)}", Severity.Success);
    }

    private void OpenCreateThemeDialog()
    {
        _newThemeDisplayName = string.Empty;
        _newThemeId = string.Empty;
        _newThemeIdAutoSuggested = true;
        _newThemeCloneFromId = ThemeState.CurrentThemeId;
        _createThemeDialogOpen = true;
    }

    private void CloseCreateThemeDialog() => _createThemeDialogOpen = false;

    private void OnNewThemeDisplayNameChanged(string value)
    {
        _newThemeDisplayName = value;
        if (_newThemeIdAutoSuggested)
        {
            _newThemeId = ThemeIdHelper.SuggestFromDisplayName(value);
        }
    }

    private void OnNewThemeIdChanged(string value)
    {
        _newThemeId = value;
        _newThemeIdAutoSuggested = string.IsNullOrWhiteSpace(value)
            || value == ThemeIdHelper.SuggestFromDisplayName(_newThemeDisplayName);
    }

    private bool CanSubmitCreateTheme()
        => ThemeIdHelper.IsValidThemeId(_newThemeId)
           && !string.IsNullOrWhiteSpace(_newThemeDisplayName);

    private async Task CreateThemeAsync()
    {
        if (!CanSubmitCreateTheme())
        {
            return;
        }

        var request = new ThemeCreateRequest(
            _newThemeId.Trim(),
            _newThemeDisplayName.Trim(),
            string.IsNullOrWhiteSpace(_newThemeCloneFromId) ? ThemeState.CurrentThemeId : _newThemeCloneFromId);

        var result = await FilePersistence.CreateAsync(request);
        if (!result.Success || string.IsNullOrWhiteSpace(result.ThemeId))
        {
            Snackbar.Add(result.ErrorMessage ?? "Kunne ikke oprette theme", Severity.Error);
            return;
        }

        _createThemeDialogOpen = false;
        ThemeState.SetTheme(result.ThemeId);
        RefreshDescriptor();
        Snackbar.Add($"Theme oprettet: {result.ThemeId}", Severity.Success);
    }

    private void OpenDeleteThemeDialog() => _deleteThemeDialogOpen = true;

    private void CloseDeleteThemeDialog() => _deleteThemeDialogOpen = false;

    private bool CanDeleteCurrentTheme()
        => FilePersistence.IsAvailable && FilePersistence.CanDeleteTheme(ThemeState.CurrentThemeId);

    private async Task DeleteCurrentThemeAsync()
    {
        var themeId = ThemeState.CurrentThemeId;
        var result = await FilePersistence.DeleteAsync(themeId);
        if (!result.Success)
        {
            Snackbar.Add(result.ErrorMessage ?? "Kunne ikke slette theme", Severity.Error);
            return;
        }

        await SessionStore.ClearSessionAsync(themeId);
        _deleteThemeDialogOpen = false;

        if (!string.IsNullOrWhiteSpace(result.FallbackThemeId))
        {
            ThemeState.SetTheme(result.FallbackThemeId);
        }

        RefreshDescriptor();
        Snackbar.Add($"Theme slettet: {themeId}", Severity.Success);
    }

    private string GetThemeFileName()
    {
        if (Catalog is IReloadableMudThemeCatalog reloadable)
        {
            return reloadable.TryGetIndexEntry(ThemeState.CurrentThemeId)?.File ?? $"{ThemeState.CurrentThemeId}.theme.json";
        }

        return $"{ThemeState.CurrentThemeId}.theme.json";
    }

    private void OpenImportDialog()
    {
        _importJson = string.Empty;
        _importDialogOpen = true;
    }

    private void CloseImportDialog() => _importDialogOpen = false;

    private async Task PasteImportJsonAsync()
    {
        try
        {
            _importJson = await JsRuntime.InvokeAsync<string>("navigator.clipboard.readText");
        }
        catch (JSException)
        {
            Snackbar.Add("Kunne ikke læse clipboard", Severity.Warning);
        }
    }

    private void ImportThemeJson()
    {
        if (!ThemeState.TryImportThemeJson(_importJson, out var errorMessage))
        {
            Snackbar.Add(errorMessage ?? "Import fejlede", Severity.Error);
            return;
        }

        _importDialogOpen = false;
        Snackbar.Add($"Theme importeret: {ThemeState.CurrentThemeId}", Severity.Success);
    }

    private void SyncExpandedGroups()
    {
        foreach (var group in ThemeTokenEditorLayout.V1)
        {
            _expandedGroups[group.Title] = group.Title switch
            {
                "Palette (light)" => !ThemeState.IsDarkMode,
                "Palette (dark)" => ThemeState.IsDarkMode,
                _ => true
            };
        }
    }

    private bool IsGroupExpanded(string title)
        => _expandedGroups.TryGetValue(title, out var expanded) && expanded;

    private void SetGroupExpanded(string title, bool expanded)
        => _expandedGroups[title] = expanded;

    private bool HasVisibleTokens()
        => ThemeTokenEditorLayout.V1.Any(GroupHasVisibleTokens);

    private bool RowHasVisibleTokens(ThemeTokenEditorRow row)
        => row.TokenIds.Any(id => IsTokenVisible(ThemeTokenEditorLayout.FindToken(id)));

    private bool SectionHasVisibleTokens(ThemeTokenEditorSection section)
        => section.Rows.Any(RowHasVisibleTokens);

    private bool GroupHasVisibleTokens(ThemeTokenEditorGroup group)
        => group.Sections.Any(SectionHasVisibleTokens);

    private bool IsTokenVisible(ThemeTokenDefinition? token)
    {
        if (token is null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(_search))
        {
            return !_showOnlyModified || ThemeState.IsTokenModified(token.Id);
        }

        var matchesSearch = token.Label.Contains(_search, StringComparison.OrdinalIgnoreCase)
            || token.Id.Contains(_search, StringComparison.OrdinalIgnoreCase)
            || (_search.Contains("ændret", StringComparison.OrdinalIgnoreCase) && ThemeState.IsTokenModified(token.Id))
            || (_search.Contains("modified", StringComparison.OrdinalIgnoreCase) && ThemeState.IsTokenModified(token.Id));

        if (!matchesSearch)
        {
            return false;
        }

        return !_showOnlyModified || ThemeState.IsTokenModified(token.Id);
    }

    private bool TryGetContrastPreview(ThemeTokenEditorRow row, out ContrastPreview preview)
    {
        preview = default!;
        if (row.TokenIds.Length != 2)
        {
            return false;
        }

        var backgroundToken = ThemeTokenEditorLayout.FindToken(row.TokenIds[0]);
        var foregroundToken = ThemeTokenEditorLayout.FindToken(row.TokenIds[1]);
        if (backgroundToken?.Kind != ThemeTokenKind.Color || foregroundToken?.Kind != ThemeTokenKind.Color)
        {
            return false;
        }

        if (!foregroundToken.Id.EndsWith("ContrastText", StringComparison.OrdinalIgnoreCase)
            && !foregroundToken.Id.EndsWith("Text", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!TryParseColor(ThemeState.GetToken(backgroundToken.Id), out var background)
            || !TryParseColor(ThemeState.GetToken(foregroundToken.Id), out var foreground))
        {
            return false;
        }

        var ratio = ThemeContrastHelper.GetContrastRatio(foreground, background);
        preview = new ContrastPreview(
            background.ToString(MudColorOutputFormats.Hex),
            foreground.ToString(MudColorOutputFormats.Hex),
            ratio,
            ThemeContrastHelper.GetWcagLabel(ratio));

        return true;
    }

    private static bool TryParseColor(string? value, out MudColor color)
    {
        if (!string.IsNullOrWhiteSpace(value) && MudColor.TryParse(value!, out var parsed))
        {
            color = parsed;
            return true;
        }

        color = new MudColor("#000000");
        return false;
    }

    private void ResetTheme()
    {
        ThemeState.ReloadTheme();
        Snackbar.Add("Nulstillet til catalog-version", Severity.Info);
    }

    private async Task CopyCSharpExportAsync()
    {
        var export = ThemeState.ExportProductionTheme();
        await JsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", export.ThemeClassFile);
        Snackbar.Add(
            $"C# kopieret ({export.ClassName}.cs, v{export.Version}, {export.UpdatedAt}). Husk også theme.manifest.json.",
            Severity.Success);
    }

    private async Task CopyManifestExportAsync()
    {
        var export = ThemeState.ExportProductionTheme();
        await JsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", export.ThemeManifestFile);
        Snackbar.Add($"theme.manifest.json kopieret (v{export.Version}, {export.UpdatedAt})", Severity.Success);
    }

    private async Task CopyJsonExportAsync()
    {
        var export = ThemeState.ExportJson();
        await JsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", export);
        Snackbar.Add("Theme JSON copied to clipboard", Severity.Success);
    }

    public void Dispose() => ThemeState.Changed -= HandleThemeChanged;

    private sealed record ContrastPreview(string BackgroundHex, string ForegroundHex, double Ratio, string WcagLabel)
    {
        public string PreviewStyle =>
            $"background-color: {BackgroundHex}; color: {ForegroundHex}; padding: 0.5rem 0.75rem; " +
            "border-radius: var(--mud-default-borderradius); font-size: 0.875rem; font-weight: 500;";

        public Color ChipColor => Ratio >= 4.5 ? Color.Success : Ratio >= 3 ? Color.Warning : Color.Error;
    }
}
