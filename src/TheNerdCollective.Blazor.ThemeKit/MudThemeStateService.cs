using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using MudBlazor;
using System.Text.Json;

namespace TheNerdCollective.Blazor.ThemeKit;

public sealed class MudThemeStateService : IMudThemeStateService
{
    private static readonly JsonSerializerOptions ImportJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    private readonly IMudThemeCatalog _catalog;
    private readonly MudThemeKitOptions _options;
    private readonly MudThemePreferencesService _preferences;
    private readonly IMudThemeSessionStore _sessionStore;
    private readonly IJSRuntime _jsRuntime;

    private bool _initialized;
    private MudTheme _currentTheme = new();
    private MudTheme _catalogBaseline = new();
    private string _currentThemeId = string.Empty;

    public MudThemeStateService(
        IMudThemeCatalog catalog,
        IOptions<MudThemeKitOptions> options,
        MudThemePreferencesService preferences,
        IMudThemeSessionStore sessionStore,
        IJSRuntime jsRuntime)
    {
        _catalog = catalog;
        _options = options.Value;
        _preferences = preferences;
        _sessionStore = sessionStore;
        _jsRuntime = jsRuntime;

        // Avoid showing bare MudBlazor defaults before InitializeAsync (localStorage) runs.
        ApplyCatalogTheme(ResolveDefaultThemeId(), persist: false);
    }

    public event Action? Changed;

    public bool IsInitialized => _initialized;

    public MudTheme CurrentTheme => _currentTheme;

    public string CurrentThemeId => _currentThemeId;

    public bool IsDarkMode { get; private set; }

    public bool HasUnsavedChanges => GetModifiedTokens().Count > 0;

    public bool HasSavedSession =>
        _sessionStore.IsLoaded && _sessionStore.TryGetSession(_currentThemeId, out _);

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        var themeId = ResolveDefaultThemeId();

        try
        {
            var storedThemeId = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", _preferences.ThemeIdKey);
            if (!string.IsNullOrWhiteSpace(storedThemeId) && _catalog.All.Any(t => t.Id == storedThemeId))
            {
                themeId = storedThemeId;
            }

            var storedDark = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", _preferences.IsDarkModeKey);
            if (bool.TryParse(storedDark, out var isDark))
            {
                IsDarkMode = isDark;
            }
        }
        catch (Exception ex) when (IsJsInteropUnavailable(ex))
        {
            // localStorage unavailable during prerender — keep defaults.
        }

        await _sessionStore.EnsureLoadedAsync();
        ApplyCatalogTheme(themeId, persist: false);
        TryApplySessionForCurrentTheme();
        _initialized = true;
        NotifyChanged();
    }

    public void SetTheme(string themeId)
    {
        if (string.IsNullOrWhiteSpace(themeId) || themeId == _currentThemeId)
        {
            return;
        }

        ApplyCatalogTheme(themeId, persist: true);
        TryApplySessionForCurrentTheme();
        NotifyChanged();
    }

    public void SetDarkMode(bool isDarkMode)
    {
        if (IsDarkMode == isDarkMode)
        {
            return;
        }

        IsDarkMode = isDarkMode;
        _ = PersistDarkModeAsync();
        NotifyChanged();
    }

    public void SetToken(string tokenId, string value)
    {
        var token = ThemeTokenRegistry.V1.FirstOrDefault(t => t.Id == tokenId);
        if (token is null)
        {
            return;
        }

        token.SetValue(_currentTheme, value);
        NotifyChanged();
    }

    public string? GetToken(string tokenId)
    {
        var token = ThemeTokenRegistry.V1.FirstOrDefault(t => t.Id == tokenId);
        return token?.GetValue(_currentTheme);
    }

    public string? GetCatalogToken(string tokenId)
    {
        var token = ThemeTokenRegistry.V1.FirstOrDefault(t => t.Id == tokenId);
        return token?.GetValue(_catalogBaseline);
    }

    public bool IsTokenModified(string tokenId)
        => TryGetTokenChange(tokenId, out _);

    public IReadOnlyList<ThemeTokenChange> GetModifiedTokens()
    {
        var changes = new List<ThemeTokenChange>();
        foreach (var token in ThemeTokenRegistry.V1)
        {
            if (TryGetTokenChange(token.Id, out var change) && change is not null)
            {
                changes.Add(change);
            }
        }

        return changes;
    }

    public void ResetToken(string tokenId)
    {
        var token = ThemeTokenRegistry.V1.FirstOrDefault(t => t.Id == tokenId);
        if (token is null)
        {
            return;
        }

        var baseline = token.GetValue(_catalogBaseline);
        if (!string.IsNullOrWhiteSpace(baseline))
        {
            token.SetValue(_currentTheme, baseline);
            NotifyChanged();
        }
    }

    public MudThemeDescriptor? GetCurrentDescriptor()
        => _catalog.All.FirstOrDefault(t => t.Id == _currentThemeId);

    public string GetSuggestedNextVersion()
    {
        var descriptor = GetCurrentDescriptor();
        return ThemeVersionHelper.BumpPatch(descriptor?.Version ?? "1.0.0");
    }

    public void ReloadTheme()
    {
        if (string.IsNullOrWhiteSpace(_currentThemeId))
        {
            return;
        }

        _ = _sessionStore.ClearSessionAsync(_currentThemeId);
        ApplyCatalogTheme(_currentThemeId, persist: false);
        NotifyChanged();
    }

    public string ExportCatalogThemeClass(string? version = null)
        => ExportProductionTheme(version).ThemeClassFile;

    public string ExportThemeManifest(string? version = null, string? mudBlazorVersion = null)
        => MudThemeExportWriter.WriteThemeManifest(CreateExportOptions(version, mudBlazorVersion));

    public MudThemeProductionExport ExportProductionTheme(string? version = null)
        => MudThemeExportWriter.WriteProductionExport(_currentTheme, CreateExportOptions(version));

    public string ExportJson(string? version = null, string? updatedAt = null)
        => MudThemeJsonSerializer.WriteThemeDocument(CreateExportDocument(version, updatedAt));

    public MudThemeJsonDocument CreateExportDocument(string? version = null, string? updatedAt = null)
    {
        var descriptor = GetCurrentDescriptor();
        return MudThemeJsonSerializer.CreateDocument(
            _currentTheme,
            _currentThemeId,
            version ?? descriptor?.Version ?? "1.0.0",
            descriptor?.DisplayName,
            updatedAt ?? descriptor?.UpdatedAt ?? ThemeVersionHelper.TodayIsoDate());
    }

    public IReadOnlyList<MudThemeSessionSummary> GetSavedSessions()
        => _sessionStore.ListSessions();

    public async Task SaveSessionAsync()
    {
        if (string.IsNullOrWhiteSpace(_currentThemeId))
        {
            return;
        }

        var descriptor = GetCurrentDescriptor();
        var document = CreateExportDocument(descriptor?.Version);
        await _sessionStore.SaveSessionAsync(_currentThemeId, document, document.Version);
        NotifyChanged();
    }

    public async Task ClearSessionAsync()
    {
        if (string.IsNullOrWhiteSpace(_currentThemeId))
        {
            return;
        }

        await _sessionStore.ClearSessionAsync(_currentThemeId);
        ApplyCatalogTheme(_currentThemeId, persist: false);
        NotifyChanged();
    }

    public bool TryImportThemeJson(string json, out string? errorMessage)
    {
        errorMessage = null;
        if (string.IsNullOrWhiteSpace(json))
        {
            errorMessage = "JSON is empty.";
            return false;
        }

        MudThemeJsonDocument? document;
        try
        {
            document = JsonSerializer.Deserialize<MudThemeJsonDocument>(json, ImportJsonOptions);
        }
        catch (JsonException ex)
        {
            errorMessage = $"Invalid JSON: {ex.Message}";
            return false;
        }

        if (document is null || document.Tokens.Count == 0)
        {
            errorMessage = "Theme JSON must include at least one token.";
            return false;
        }

        var themeId = ResolveThemeId(document.Id);
        _currentThemeId = themeId;
        _currentTheme = MudThemeJsonSerializer.ApplyDocument(_catalog.GetTheme(themeId), document);
        NotifyChanged();
        return true;
    }

    private void TryApplySessionForCurrentTheme()
    {
        if (!_sessionStore.TryGetSession(_currentThemeId, out var session) || session is null)
        {
            return;
        }

        _currentTheme = MudThemeJsonSerializer.ApplyDocument(_currentTheme, session.Document);
    }

    private void ApplyCatalogTheme(string themeId, bool persist)
    {
        _currentThemeId = themeId;
        _currentTheme = MudThemeCloner.Clone(_catalog.GetTheme(themeId));
        _catalogBaseline = MudThemeCloner.Clone(_currentTheme);

        if (persist)
        {
            _ = PersistThemeIdAsync(themeId);
        }
    }

    private bool TryGetTokenChange(string tokenId, out ThemeTokenChange? change)
    {
        change = null;
        var token = ThemeTokenRegistry.V1.FirstOrDefault(t => t.Id == tokenId);
        if (token is null)
        {
            return false;
        }

        var current = token.GetValue(_currentTheme);
        var baseline = token.GetValue(_catalogBaseline);
        if (string.Equals(current, baseline, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        change = new ThemeTokenChange(token.Id, token.Label, baseline, current);
        return true;
    }

    private string ResolveThemeId(string? themeId)
    {
        if (!string.IsNullOrWhiteSpace(themeId) && _catalog.All.Any(t => t.Id == themeId))
        {
            return themeId;
        }

        return string.IsNullOrWhiteSpace(_currentThemeId)
            ? ResolveDefaultThemeId()
            : _currentThemeId;
    }

    private async Task PersistThemeIdAsync(string themeId)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", _preferences.ThemeIdKey, themeId);
        }
        catch (Exception ex) when (IsJsInteropUnavailable(ex))
        {
            // Ignore when storage is unavailable.
        }
    }

    private async Task PersistDarkModeAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", _preferences.IsDarkModeKey, IsDarkMode.ToString().ToLowerInvariant());
        }
        catch (Exception ex) when (IsJsInteropUnavailable(ex))
        {
            // Ignore when storage is unavailable.
        }
    }

    private MudThemeCatalogExportOptions CreateExportOptions(string? version = null, string? mudBlazorVersion = null)
    {
        var descriptor = GetCurrentDescriptor();
        var exportVersion = version
            ?? (HasUnsavedChanges ? GetSuggestedNextVersion() : descriptor?.Version ?? "1.0.0");

        return new MudThemeCatalogExportOptions(
            Id: _currentThemeId,
            DisplayName: descriptor?.DisplayName ?? _currentThemeId,
            Version: exportVersion,
            UpdatedAt: ThemeVersionHelper.TodayIsoDate(),
            MudBlazorVersion: mudBlazorVersion ?? "9.5",
            SourceNotes: descriptor?.Source);
    }

    private void NotifyChanged() => Changed?.Invoke();

    private string ResolveDefaultThemeId()
    {
        if (!string.IsNullOrWhiteSpace(_options.DefaultThemeId))
        {
            return _options.DefaultThemeId;
        }

        return _catalog.DefaultThemeId;
    }

    private static bool IsJsInteropUnavailable(Exception ex)
        => ex is JSException or InvalidOperationException or JSDisconnectedException;
}
