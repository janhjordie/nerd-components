using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace TheNerdCollective.MudComponents.Changelog;

public partial class NerdChangelog
{
    private static readonly IReadOnlyList<string> AllChangeTypes = ["major", "minor", "patch"];

    [Inject]
    private NerdChangelogService ChangelogService { get; set; } = default!;

    private IReadOnlyList<NerdChangelogEntry>? _entries;
    private List<NerdChangelogEntry> _filtered = [];
    private string _search = string.Empty;
    private IReadOnlyCollection<string> _selectedTypes = new HashSet<string>(AllChangeTypes, StringComparer.OrdinalIgnoreCase);

    protected override async Task OnInitializedAsync()
    {
        _entries = await ChangelogService.GetEntriesAsync();
        ApplyFilter();
    }

    private void OnFilterChanged() => ApplyFilter();

    private void ApplyFilter()
    {
        if (_entries is null)
        {
            _filtered = [];
            return;
        }

        var query = _search.Trim();
        var types = _selectedTypes is { Count: > 0 }
            ? _selectedTypes
            : AllChangeTypes;

        _filtered = _entries
            .Where(e => types.Contains(e.ChangeType, StringComparer.OrdinalIgnoreCase))
            .Where(e => MatchesSearch(e, query))
            .ToList();
    }

    private static bool MatchesSearch(NerdChangelogEntry entry, string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return true;
        }

        return Contains(entry.Title, query)
               || Contains(entry.Description, query)
               || Contains(entry.Version, query)
               || Contains(entry.ChangeType, query);
    }

    private static bool Contains(string? haystack, string needle) =>
        !string.IsNullOrEmpty(haystack)
        && haystack.Contains(needle, StringComparison.OrdinalIgnoreCase);

    private bool ShouldInitiallyExpand(NerdChangelogEntry entry)
    {
        if (_filtered.Count == 0)
        {
            return false;
        }

        var index = _filtered.IndexOf(entry);
        return index >= 0 && index < 3;
    }

    private static Color GetChangeTypeColor(string changeType) => changeType?.ToLowerInvariant() switch
    {
        "major" => Color.Error,
        "minor" => Color.Warning,
        "patch" => Color.Success,
        _ => Color.Default
    };
}
