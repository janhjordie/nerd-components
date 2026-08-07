using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace TheNerdCollective.MudComponents.Changelog;

public partial class NerdChangelog
{
    [Inject]
    private NerdChangelogService ChangelogService { get; set; } = default!;

    private IReadOnlyList<NerdChangelogEntry>? _entries;

    protected override async Task OnInitializedAsync()
    {
        _entries = await ChangelogService.GetEntriesAsync();
    }

    private bool ShouldInitiallyExpand(NerdChangelogEntry entry)
    {
        if (_entries is null || _entries.Count == 0)
        {
            return false;
        }

        var index = _entries.ToList().IndexOf(entry);
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
