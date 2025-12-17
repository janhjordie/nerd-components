# TheNerdCollective.MudComponents.Timesheet

A MudBlazor component for displaying and managing timesheet entries from the GetHarvest API.

## Overview

TheNerdCollective.MudComponents.Timesheet provides a beautiful, interactive Blazor component that displays timesheet data with month navigation, hourly summaries, and billable vs. unbilled hour tracking.

## Quick Start

### Installation

```bash
dotnet add package TheNerdCollective.MudComponents.Timesheet
```

### Setup

1. **Add Script Reference** in `App.razor` (if using MudBlazor):
```html
<head>
    <link href="https://fonts.googleapis.com/css?family=Roboto:300,400,500,700&display=swap" rel="stylesheet" />
    <link href="https://cdn.jsdelivr.net/npm/@fontsource/roboto@4.5.0/index.min.css" rel="stylesheet" />
    <link href="https://cdn.jsdelivr.net/npm/mudc@latest/dist/mudc.min.css" rel="stylesheet" />
</head>
```

2. **Configure Harvest Integration** (required):
```json
{
  "Harvest": {
    "ApiToken": "your_api_token",
    "AccountId": "your_account_id",
    "ProjectIds": [123456, 789012]
  }
}
```

3. **Register Services** in Program.cs:
```csharp
using MudBlazor.Services;
using TheNerdCollective.Integrations.Harvest.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMudServices();
builder.Services.AddHarvestIntegration(builder.Configuration);

var app = builder.Build();
```

4. **Use the Component**:
```razor
@page "/timesheets"
@using TheNerdCollective.MudComponents.Timesheet

<TimesheetDisplay />
```

## Features

- **Month Navigation** - Navigate between months to view different timesheet periods
- **Entry Details** - View dates, projects, tasks, users, and notes
- **Billable Tracking** - Automatically separates billable and unbilled hours
- **Hour Summaries** - Displays total hours, billable hours, and unbilled hours
- **Responsive Design** - Adapts to mobile, tablet, and desktop screens
- **Loading States** - Shows loading indicator while fetching data
- **Empty States** - Handles empty timesheet periods gracefully

## Component Parameters

### `InitialDate`
Sets the initial month to display. Defaults to the current month.

```razor
<TimesheetDisplay InitialDate="new DateTime(2025, 12, 1)" />
```

### `UnbilledKeyword`
The keyword used to identify unbilled/without payment hours in task names. Defaults to `"(U/B)"` (Danish: uden betaling).

```razor
<!-- Use default "(U/B)" -->
<TimesheetDisplay />

<!-- Use custom keyword -->
<TimesheetDisplay UnbilledKeyword="[UNPAID]" />
```

## Customization

### Styling
The component uses MudBlazor's theming system. Customize colors and spacing through your MudBlazor theme configuration.

### Unbilled Hours Keyword
By default, tasks containing "(U/B)" (Danish: uden betaling - without payment) are classified as unbilled. Use the `UnbilledKeyword` parameter to customize this:

```razor
<!-- Use custom keyword for identifying unbilled hours -->
<TimesheetDisplay UnbilledKeyword="[NO-PAY]" />
```

## Required Dependencies

- **TheNerdCollective.Integrations.Harvest** 0.1.0+
- **MudBlazor** 8.15+
- **Blazor** (Web App)
- **.NET** 10.0+

## Usage Example

### Complete Integration
```razor
@page "/timesheets"
@using TheNerdCollective.MudComponents.Timesheet

<PageTitle>Timesheets</PageTitle>

<TimesheetDisplay InitialDate="new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1)" />

@code {
    // Component handles all logic internally
}
```

### In a Page with Other Content
```razor
@page "/dashboard"
@using TheNerdCollective.MudComponents.Timesheet

<MudContainer>
    <MudText Typo="Typo.h3" Class="mb-6">My Dashboard</MudText>
    
    <MudTabs>
        <MudTabPanel Text="Overview">
            <!-- Dashboard content -->
        </MudTabPanel>
        <MudTabPanel Text="Timesheets">
            <TimesheetDisplay />
        </MudTabPanel>
    </MudTabs>
</MudContainer>
```

## Data Flow

1. Component initializes with the current month (or specified date)
2. `LoadTimesheets()` is called, which retrieves timesheet entries from Harvest API
3. Entries are displayed in a sortable table with month navigation
4. Summary statistics are calculated and displayed
5. User can navigate between months using Previous/Next buttons

## Error Handling

The component gracefully handles errors by:
- Catching API exceptions silently
- Displaying an empty state if no data is available
- Showing loading states during data retrieval

For production use, consider extending error handling to show user-friendly error messages.

## License

Apache License 2.0 - See LICENSE file for details

## Repository

[TheNerdCollective.Components on GitHub](https://github.com/janhjordie/TheNerdCollective.Components)

---

Built with ❤️ by [The Nerd Collective Aps](https://www.thenerdcollective.dk/)
