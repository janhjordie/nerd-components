namespace TheNerdCollective.MudComponents.Changelog;

public sealed class NerdChangelogOptions
{
    /// <summary>Directory containing <c>changelog.json</c> / <c>changelog-N.json</c>.</summary>
    public string? DataDirectory { get; set; }

    /// <summary>Max entries per JSON file before agents must create the next suffix.</summary>
    public int MaxEntriesPerFile { get; set; } = 50;

    /// <summary>Enable the injectable changelog page (default true).</summary>
    public bool EnableChangelogPage { get; set; } = true;

    /// <summary>
    /// Public URL for the changelog page (hub nav, AppBar, auth allow-list).
    /// Default <c>/nerd-changelog</c>. Override freely (e.g. <c>/changelog</c>).
    /// The Blazor <c>@page</c> stays canonical at <c>/nerd-changelog</c>;
    /// <see cref="NerdChangelogEndpointExtensions.UseNerdChangelogRouteOverride"/> maps this path.
    /// </summary>
    public string ChangelogRoute { get; set; } = NerdChangelogEndpointExtensions.CanonicalRoute;
}
