// Licensed under the Apache License, Version 2.0.
// See LICENSE file in the project root for full license information.

namespace TheNerdCollective.Integrations.Brevo;

/// <summary>
/// Outcome of a Brevo API call.
/// </summary>
public sealed record BrevoResult(bool Succeeded, int StatusCode, string? Error)
{
    /// <summary>
    /// Gets a successful result.
    /// </summary>
    public static BrevoResult Ok(int statusCode) => new(true, statusCode, null);

    /// <summary>
    /// Gets a failed result.
    /// </summary>
    public static BrevoResult Fail(int statusCode, string? error) => new(false, statusCode, error);
}
