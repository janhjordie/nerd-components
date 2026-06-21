using System;
using System.Threading;
using System.Threading.Tasks;
using TheNerdCollective.Integrations.Dar.Models;
using TheNerdCollective.Integrations.Dar.Services.Dar;

namespace TheNerdCollective.Integrations.Dar.Services;

/// <summary>
/// Id-baseret adresse-flow: autocomplete → bedste match → KvHxInput og BBR-nøgler.
/// </summary>
public sealed class DarAddressResolutionService
{
    private readonly DarAddressAutocompleteService _autocomplete;
    private readonly DarAdresseopslagService _adresseopslag;

    public DarAddressResolutionService(
        DarAddressAutocompleteService autocomplete,
        DarAdresseopslagService adresseopslag)
    {
        _autocomplete = autocomplete ?? throw new ArgumentNullException(nameof(autocomplete));
        _adresseopslag = adresseopslag ?? throw new ArgumentNullException(nameof(adresseopslag));
    }

    /// <summary>
    /// Søger adressen, vælger bedste match (inkl. enhed med etage/dør) og returnerer ids + KvHxInput.
    /// </summary>
    public async Task<DarAddressResolutionResult> ResolveBestMatchAsync(
        string address,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            throw new ArgumentException("Adresse må ikke være tom.", nameof(address));
        }

        var results = await _autocomplete.SearchAsync(address, cancellationToken).ConfigureAwait(false);
        var selection = DanishAddressAutocompleteMatching.ResolveBestMatch(results, address)
            ?? throw new InvalidOperationException("Ingen DAR autocomplete-resultater fundet for adressen.");

        var adresseopslag = await _adresseopslag.LookupFromAutocompleteAsync(selection, cancellationToken)
            .ConfigureAwait(false);

        return new DarAddressResolutionResult
        {
            Ids = DarAddressIds.FromSelection(selection),
            Selection = selection,
            Adresseopslag = adresseopslag
        };
    }

    /// <summary>
    /// Henter KvHxInput via DAR-id'er fra autocomplete (ikke DisplayName).
    /// </summary>
    public Task<KvHxInputDto> GetKvHxInputByLocalIdAsync(
        string localId,
        string resultType,
        string? husnummerId = null,
        CancellationToken cancellationToken = default) =>
        GetKvHxInputAsync(
            new DarAddressIds
            {
                LocalId = localId,
                ResultType = resultType,
                HusnummerId = husnummerId
            },
            cancellationToken);

    /// <summary>Henter KvHxInput via <see cref="DarAddressIds"/>.</summary>
    public async Task<KvHxInputDto> GetKvHxInputAsync(
        DarAddressIds ids,
        CancellationToken cancellationToken = default)
    {
        var adresseopslag = await GetAdresseopslagAsync(ids, cancellationToken).ConfigureAwait(false);
        return adresseopslag.KvHxInput;
    }

    /// <summary>Fuldt adresseopslag via DAR-id'er fra autocomplete.</summary>
    public Task<AdresseopslagResult> GetAdresseopslagAsync(
        DarAddressIds ids,
        CancellationToken cancellationToken = default)
    {
        if (ids == null)
        {
            throw new ArgumentNullException(nameof(ids));
        }

        return _adresseopslag.LookupByHusnummerIdAsync(
            ids.GetHusnummerIdForLookup(),
            ids.GetAdresseLocalId(),
            cancellationToken);
    }
}
