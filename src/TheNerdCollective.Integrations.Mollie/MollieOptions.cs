// Licensed under the Apache License, Version 2.0.
// See LICENSE file in the project root for full license information.

namespace TheNerdCollective.Integrations.Mollie;

/// <summary>
/// Configuration for Mollie's Payments API.
/// </summary>
public sealed class MollieOptions
{
    /// <summary>
    /// The default configuration section name.
    /// </summary>
    public const string DefaultSectionName = "Mollie";

    /// <summary>
    /// Gets or sets the Mollie API key.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
}
