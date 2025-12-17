// Licensed under the Apache License, Version 2.0.
// See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Components;
using TheNerdCollective.Integrations.Harvest;
using TheNerdCollective.Integrations.Harvest.Models;

namespace TheNerdCollective.MudComponents.HarvestTimesheet;

/// <summary>
/// A Blazor component for displaying timesheet entries from Harvest API.
/// </summary>
public partial class TimesheetDisplay : ComponentBase
{
    [Inject] private HarvestService HarvestService { get; set; } = null!;

    private List<TimesheetEntry> Timesheets = new();
    private DateTime? SelectedDate = DateTime.Now;
    private bool IsLoading = false;
    private bool ShowDebugPanel = false;

    /// <summary>
    /// Gets or sets the date to display timesheets for. Defaults to current month.
    /// </summary>
    [Parameter]
    public DateTime? InitialDate { get; set; }

    /// <summary>
    /// Gets or sets the keyword used to identify unbilled/unpaid hours in task names.
    /// Default is "(U/B)".
    /// </summary>
    [Parameter]
    public string UnbilledKeyword { get; set; } = "(U/B)";

    protected override async Task OnInitializedAsync()
    {
        var now = InitialDate ?? DateTime.Now;
        SelectedDate = new DateTime(now.Year, now.Month, 1);
        await LoadTimesheets();
    }

    private async Task LoadTimesheets()
    {
        if (!SelectedDate.HasValue)
        {
            return;
        }

        IsLoading = true;

        try
        {
            var date = SelectedDate.Value;
            var firstDay = new DateTime(date.Year, date.Month, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);

            Timesheets = await HarvestService.GetTimesheetEntriesAsync(firstDay, lastDay);
        }
        catch
        {
            // Error handling can be extended here
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task PreviousMonth()
    {
        if (SelectedDate.HasValue)
        {
            SelectedDate = SelectedDate.Value.AddMonths(-1);
            await LoadTimesheets();
        }
    }

    private async Task NextMonth()
    {
        if (SelectedDate.HasValue)
        {
            SelectedDate = SelectedDate.Value.AddMonths(1);
            await LoadTimesheets();
        }
    }

    private decimal TotalHours => Timesheets.Sum(t => t.Hours);

    private decimal UnbilledHours => Timesheets
        .Where(t => t.TaskName?.Contains(UnbilledKeyword) ?? false)
        .Sum(t => t.Hours);

    private decimal BillableHours => TotalHours - UnbilledHours;

    private static bool IsTrelloExternal(TimesheetEntry entry)
    {
        return (entry.ExternalReferenceService?.IndexOf("trello", StringComparison.OrdinalIgnoreCase) ?? -1) >= 0;
    }
}
