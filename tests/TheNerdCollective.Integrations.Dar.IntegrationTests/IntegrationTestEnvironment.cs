using System.Text.Json;
using TheNerdCollective.Integrations.Dar;
using TheNerdCollective.Integrations.Dar.Configuration;
using TheNerdCollective.Integrations.Dar.Services;

namespace TheNerdCollective.Integrations.Dar.IntegrationTests;

internal static class IntegrationTestEnvironment
{
    private const string TestApiKey =
        "J1MYwhpYJ8NRdHaatF8v5Rrs73oYyapMbp0yQGOvlY178YafxbnpeN9iJLOPQehck3j53CJLSuCRPOufDnuWQk9iiHGFTCxd5";

    internal const string TestStreetAndNumber = "Århusvej 69a";
    internal const string TestPostalCode = "3000";
    internal const string TestCity = "Helsingør";
    internal const string TestFullAddress = "Århusvej 69a, 3000 Helsingør";

    internal static string ApiKey
    {
        get
        {
            var fromEnv = Environment.GetEnvironmentVariable("TheNerdCollective__Dar__ApiKey")
                ?? Environment.GetEnvironmentVariable("DATAFORDELER_API_KEY");

            if (IsUsableApiKey(fromEnv))
            {
                return fromEnv!;
            }

            var fromLocalConfig = TryReadApiKeyFromTestWebConfig();
            if (IsUsableApiKey(fromLocalConfig))
            {
                return fromLocalConfig!;
            }

            return TestApiKey;
        }
    }

    internal static DarServices CreateServices() =>
        DarClientFactory.Create(
            new DarOptions { ApiKey = ApiKey },
            new HttpClient());

    private static bool IsUsableApiKey(string? apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return false;
        }

        return !apiKey.Contains("din-api", StringComparison.OrdinalIgnoreCase)
            && !apiKey.Contains("your-", StringComparison.OrdinalIgnoreCase)
            && !apiKey.Equals("changeme", StringComparison.OrdinalIgnoreCase);
    }

    private static string? TryReadApiKeyFromTestWebConfig()
    {
        var testWebConfigPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..",
            "src", "TheNerdCollective.Integrations.Dar.TestWeb", "appsettings.local.json"));

        if (!File.Exists(testWebConfigPath))
        {
            return null;
        }

        using var document = JsonDocument.Parse(File.ReadAllText(testWebConfigPath));
        if (!document.RootElement.TryGetProperty("TheNerdCollective", out var theNerdCollective)
            || !theNerdCollective.TryGetProperty("Dar", out var dar)
            || !dar.TryGetProperty(nameof(DarOptions.ApiKey), out var apiKey))
        {
            return null;
        }

        return apiKey.GetString();
    }
}
