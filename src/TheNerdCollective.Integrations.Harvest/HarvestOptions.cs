// Licensed under the Apache License, Version 2.0.
// See LICENSE file in the project root for full license information.

namespace TheNerdCollective.Integrations.Harvest;

/// <summary>
/// Configuration options for Harvest API integration.
/// Configure these in appsettings.json under the "Harvest" section.
/// </summary>
public class HarvestOptions
{
    /// <summary>
    /// Gets or sets the Harvest API token.
    /// Generate from: https://help.getharvest.com/api-v2/authentication-api/authentication/authentication/
    /// </summary>
    public string ApiToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Harvest Account ID.
    /// Found in your Harvest account settings.
    /// </summary>
    public string AccountId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of project IDs to track.
    /// Leave empty to manually add projects via AddProjectId().
    /// </summary>
    public List<long> ProjectIds { get; set; } = new();

    /// <summary>
    /// Gets or sets an optional password to protect the timesheet view.
    /// Leave empty to disable password protection.
    /// </summary>
    public string? Password { get; set; }
}
