using System;
using System.Net.Http;
using TheNerdCollective.Integrations.Dar.Configuration;
using TheNerdCollective.Integrations.Dar.GraphQL;
using TheNerdCollective.Integrations.Dar.Services;
using TheNerdCollective.Integrations.Dar.Services.Bbr;
using TheNerdCollective.Integrations.Dar.Services.Dar;
using TheNerdCollective.Integrations.Dar.Services.Dar.Internal;
using TheNerdCollective.Integrations.Dar.Services.Internal;

namespace TheNerdCollective.Integrations.Dar
{
    /// <summary>
    /// Factory til oprettelse af DAR- og BBR-services.
    /// </summary>
    public static class DarClientFactory
    {
        /// <summary>
        /// Opretter kun autocomplete-service (kræver ikke Datafordeler API-nøgle).
        /// </summary>
        public static DarAddressAutocompleteService CreateAutocomplete(
            DarAutocompleteOptions options,
            HttpClient? httpClient = null)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return new DarAddressAutocompleteService(options, httpClient ?? new HttpClient());
        }

        /// <summary>
        /// Opretter <see cref="DarServices"/> med separate services pr. dataområde.
        /// </summary>
        public static DarServices Create(DarOptions options, HttpClient? httpClient = null)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (string.IsNullOrWhiteSpace(options.ApiKey))
            {
                throw new ArgumentException("Datafordeler API-nøgle mangler.", nameof(options));
            }

            var client = httpClient ?? new HttpClient();
            var graphQlClient = new DatafordelerGraphQlClient(client, options.ApiKey);
            var accessor = new GraphQlDataAccessor(
                graphQlClient,
                options.BbrGraphQlUrl,
                options.DarGraphQlUrl,
                options.DagiGraphQlUrl);

            var regionFetcher = new DagiRegionDataFetcher(accessor, client, options.ApiKey, options.Dagi);
            var region = new DarRegionService(regionFetcher);

            var dar = new DarRegister(
                new DarAdresseopslagService(accessor),
                new DarHusnummerService(accessor),
                new DarAddressAutocompleteService(options.Autocomplete, client),
                new DarKommuneService(accessor, client, options.ApiKey, options.Dagi, region),
                region,
                new DarPostnummerService(accessor, client, options.ApiKey, options.Postnummer));

            var bbr = new BbrServices(
                new BbrBygningService(accessor),
                new BbrEnhedService(accessor),
                new BbrEtageService(accessor),
                new BbrOpgangService(accessor),
                new BbrTekniskAnlaegService(accessor),
                new BbrGrundService(accessor),
                new BbrEjendomsrelationService(accessor));

            return new DarServices(dar, bbr);
        }
    }
}
