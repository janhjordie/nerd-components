using TheNerdCollective.Integrations.Dar;
using TheNerdCollective.Integrations.Dar.Configuration;
using TheNerdCollective.Integrations.Dar.Services;
using TheNerdCollective.Integrations.Dar.Services.Dar;
using Microsoft.Extensions.Options;

namespace TheNerdCollective.Integrations.Dar.TestWeb.Services;

public sealed class DarRuntime(IOptions<DarOptions> options, IHttpClientFactory httpClientFactory)
{
    public string? ConfigurationError
    {
        get
        {
            if (string.IsNullOrWhiteSpace(options.Value.ApiKey))
            {
                return
                    "Datafordeler API-nøgle mangler. Kopiér appsettings.local.json.example til appsettings.local.json " +
                    "eller sæt miljøvariablen TheNerdCollective__Dar__ApiKey.";
            }

            return null;
        }
    }

    public DarServices CreateServices() =>
        DarClientFactory.Create(options.Value, httpClientFactory.CreateClient("Datafordeler"));

    public DarAddressAutocompleteService CreateAutocomplete() =>
        new(options.Value.Autocomplete, httpClientFactory.CreateClient("Adressevaelger"));
}
