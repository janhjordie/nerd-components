// Licensed under the Apache License, Version 2.0.
// See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using TheNerdCollective.Integrations.Harvest.Models;

namespace TheNerdCollective.Integrations.Harvest;

/// <summary>
/// Service for interacting with GetHarvest.com API v2.
/// https://help.getharvest.com/api-v2/
/// </summary>
public class HarvestService
{
    private readonly HttpClient _httpClient;
    private readonly HarvestOptions _options;
    private readonly List<long> _projectIds;

    private const string HARVEST_BASE_URL = "https://api.harvestapp.com/v2/";

    public HarvestService(HttpClient httpClient, IOptions<HarvestOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _projectIds = new List<long>(_options.ProjectIds);

        ConfigureHttpClient();
    }

    private void ConfigureHttpClient()
    {
        _httpClient.BaseAddress = new Uri(HARVEST_BASE_URL);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.ApiToken}");
        _httpClient.DefaultRequestHeaders.Add("Harvest-Account-ID", _options.AccountId);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "TheNerdCollective.Integrations.Harvest/0.1.0");
    }

    /// <summary>
    /// Get timesheet entries for specified projects and date range.
    /// </summary>
    public async Task<List<TimesheetEntry>> GetTimesheetEntriesAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var allEntries = new List<TimesheetEntry>();

            foreach (var projectId in _projectIds)
            {
                var entries = await GetTimesheetEntriesByProjectAsync(projectId, startDate, endDate);
                allEntries.AddRange(entries);
            }

            return allEntries.OrderBy(e => e.SpentDate).ToList();
        }
        catch
        {
            return new List<TimesheetEntry>();
        }
    }

    /// <summary>
    /// Get timesheet entries for a specific project within a date range.
    /// </summary>
    private async Task<List<TimesheetEntry>> GetTimesheetEntriesByProjectAsync(long projectId, DateTime startDate, DateTime endDate)
    {
        try
        {
            var from = startDate.ToString("yyyy-MM-dd");
            var to = endDate.ToString("yyyy-MM-dd");

            var url = $"time_entries?project_id={projectId}&from={from}&to={to}&per_page=100";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var contentStr = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var data = JsonSerializer.Deserialize<HarvestTimeEntriesResponse>(contentStr, options);
            var entries = data?.TimeEntries ?? new List<HarvestTimeEntry>();

            return entries.Select(MapToTimesheetEntry).ToList();
        }
        catch
        {
            return new List<TimesheetEntry>();
        }
    }

    /// <summary>
    /// Get all available projects.
    /// </summary>
    public async Task<List<HarvestProject>> GetProjectsAsync()
    {
        try
        {
            var url = "projects?per_page=100";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var contentStr = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<HarvestProjectsResponse>(contentStr, options);
            return data?.Projects ?? new List<HarvestProject>();
        }
        catch
        {
            return new List<HarvestProject>();
        }
    }

    /// <summary>
    /// Add a project ID to track.
    /// </summary>
    public void AddProjectId(long projectId)
    {
        if (!_projectIds.Contains(projectId))
        {
            _projectIds.Add(projectId);
        }
    }

    /// <summary>
    /// Remove a project ID from tracking.
    /// </summary>
    public void RemoveProjectId(long projectId)
    {
        _projectIds.Remove(projectId);
    }

    /// <summary>
    /// Get all tracked project IDs.
    /// </summary>
    public List<long> GetProjectIds() => new(_projectIds);

    /// <summary>
    /// Clear all tracked project IDs.
    /// </summary>
    public void ClearProjectIds() => _projectIds.Clear();

    private TimesheetEntry MapToTimesheetEntry(HarvestTimeEntry entry)
    {
        return new TimesheetEntry
        {
            Id = entry.Id,
            ProjectId = entry.Project?.Id ?? 0,
            TaskId = entry.Task?.Id ?? 0,
            UserId = entry.User?.Id ?? 0,
            UserName = entry.User?.Name ?? string.Empty,
            ProjectName = entry.Project?.Name ?? string.Empty,
            TaskName = entry.Task?.Name ?? string.Empty,
            Notes = entry.Notes ?? string.Empty,
            ExternalReferencePermalink = entry.ExternalReference?.Permalink ?? string.Empty,
            ExternalReferenceService = entry.ExternalReference?.Service ?? string.Empty,
            Hours = entry.Hours,
            SpentDate = !string.IsNullOrEmpty(entry.SpentDate) ? DateTime.Parse(entry.SpentDate) : DateTime.Now,
            CreatedAt = entry.CreatedAt,
            UpdatedAt = entry.UpdatedAt
        };
    }
}
