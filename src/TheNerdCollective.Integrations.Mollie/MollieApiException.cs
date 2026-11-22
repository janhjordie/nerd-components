// Licensed under the Apache License, Version 2.0.
// See LICENSE file in the project root for full license information.

using System.Net;

namespace TheNerdCollective.Integrations.Mollie;

/// <summary>
/// Represents an unsuccessful response from Mollie's API.
/// </summary>
public sealed class MollieApiException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MollieApiException"/> class.
    /// </summary>
    public MollieApiException(HttpStatusCode statusCode)
        : base($"Mollie API request failed with HTTP {(int)statusCode} ({statusCode}).")
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// Gets the HTTP status code returned by Mollie.
    /// </summary>
    public HttpStatusCode StatusCode { get; }
}
