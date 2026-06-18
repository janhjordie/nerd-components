using System.Text.Json;
using Microsoft.JSInterop;

namespace TheNerdCollective.Blazor.ThemeKit;

public sealed class MudThemeSessionStore : IMudThemeSessionStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        PropertyNameCaseInsensitive = true,
    };

    private readonly MudThemePreferencesService _preferences;
    private readonly IJSRuntime _jsRuntime;
    private MudThemeSessionCollection _collection = new();
    private bool _loaded;

    public MudThemeSessionStore(MudThemePreferencesService preferences, IJSRuntime jsRuntime)
    {
        _preferences = preferences;
        _jsRuntime = jsRuntime;
    }

    public event Action? Changed;

    public bool IsLoaded => _loaded;

    public async Task EnsureLoadedAsync()
    {
        if (_loaded)
        {
            return;
        }

        _collection = await ReadCollectionAsync();
        _loaded = true;
    }

    public IReadOnlyList<MudThemeSessionSummary> ListSessions()
        => _collection.Sessions
            .OrderByDescending(pair => pair.Value.SavedAtUtc, StringComparer.Ordinal)
            .Select(pair => new MudThemeSessionSummary(pair.Key, pair.Value.Version, pair.Value.SavedAtUtc))
            .ToList();

    public bool TryGetSession(string themeId, out MudThemeSession? session)
    {
        if (string.IsNullOrWhiteSpace(themeId))
        {
            session = null;
            return false;
        }

        return _collection.Sessions.TryGetValue(themeId, out session);
    }

    public async Task SaveSessionAsync(string themeId, MudThemeJsonDocument document, string version)
    {
        await EnsureLoadedAsync();

        _collection.Sessions[themeId] = new MudThemeSession
        {
            SavedAtUtc = DateTime.UtcNow.ToString("o"),
            Version = version,
            Document = document,
        };

        await WriteCollectionAsync(_collection);
        Changed?.Invoke();
    }

    public async Task ClearSessionAsync(string themeId)
    {
        await EnsureLoadedAsync();

        if (!_collection.Sessions.Remove(themeId))
        {
            return;
        }

        await WriteCollectionAsync(_collection);
        Changed?.Invoke();
    }

    private async Task<MudThemeSessionCollection> ReadCollectionAsync()
    {
        try
        {
            var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", _preferences.ThemeSessionsKey);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new MudThemeSessionCollection();
            }

            return JsonSerializer.Deserialize<MudThemeSessionCollection>(json, JsonOptions)
                   ?? new MudThemeSessionCollection();
        }
        catch (Exception ex) when (IsJsInteropUnavailable(ex))
        {
            return new MudThemeSessionCollection();
        }
    }

    private async Task WriteCollectionAsync(MudThemeSessionCollection collection)
    {
        try
        {
            var json = JsonSerializer.Serialize(collection, JsonOptions);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", _preferences.ThemeSessionsKey, json);
        }
        catch (Exception ex) when (IsJsInteropUnavailable(ex))
        {
            // Ignore when storage is unavailable.
        }
    }

    private static bool IsJsInteropUnavailable(Exception ex)
        => ex is JSException or InvalidOperationException or JSDisconnectedException;
}
