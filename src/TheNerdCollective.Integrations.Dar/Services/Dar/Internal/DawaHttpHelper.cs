using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace TheNerdCollective.Integrations.Dar.Services.Dar.Internal;

/// <summary>
/// DAWA (api.dataforsyningen.dk) is being retired; treat Gone as "no data" so Datafordeler fallbacks can run.
/// </summary>
internal static class DawaHttpHelper
{
    public static bool IsUnavailableStatus(HttpStatusCode statusCode) =>
        statusCode == HttpStatusCode.Gone || statusCode == HttpStatusCode.ServiceUnavailable;

    /// <summary>
    /// Returns true when the response is successful; false when DAWA is permanently/temporarily unavailable (410/503).
    /// Throws for other error status codes.
    /// </summary>
    public static async Task<bool> TryEnsureSuccessAsync(HttpResponseMessage response, string sourceName)
    {
        if (response.IsSuccessStatusCode)
        {
            return true;
        }

        if (IsUnavailableStatus(response.StatusCode))
        {
            return false;
        }

        var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        throw new InvalidOperationException(
            $"{sourceName} returnerede HTTP {(int)response.StatusCode}: {body}");
    }
}
