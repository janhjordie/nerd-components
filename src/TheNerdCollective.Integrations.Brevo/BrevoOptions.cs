// Licensed under the Apache License, Version 2.0.
// See LICENSE file in the project root for full license information.

namespace TheNerdCollective.Integrations.Brevo;

/// <summary>
/// Configuration for the Brevo API. Bind from the "Brevo" configuration section.
/// </summary>
public sealed class BrevoOptions
{
    /// <summary>
    /// Gets or sets the Brevo API key sent in the api-key header.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Brevo API base address. Defaults to https://api.brevo.com/v3/.
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.brevo.com/v3/";
}
