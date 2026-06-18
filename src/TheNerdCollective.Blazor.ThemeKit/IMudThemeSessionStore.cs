namespace TheNerdCollective.Blazor.ThemeKit;

public interface IMudThemeSessionStore
{
    event Action? Changed;

    bool IsLoaded { get; }

    Task EnsureLoadedAsync();

    IReadOnlyList<MudThemeSessionSummary> ListSessions();

    bool TryGetSession(string themeId, out MudThemeSession? session);

    Task SaveSessionAsync(string themeId, MudThemeJsonDocument document, string version);

    Task ClearSessionAsync(string themeId);
}
