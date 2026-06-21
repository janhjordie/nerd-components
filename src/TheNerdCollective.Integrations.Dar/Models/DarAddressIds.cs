using System;

namespace TheNerdCollective.Integrations.Dar.Models;

/// <summary>
/// DAR-id'er fra autocomplete — eneste gyldige nøgler til adresseopslag, KvHxInput og BBR.
/// </summary>
public sealed record DarAddressIds
{
    /// <summary>
    /// Autocomplete <c>LocalId</c> — husnummer-id for adgangsadresse, adresse-id for enhed.
    /// </summary>
    public required string LocalId { get; init; }

    /// <summary><c>husnummer</c> eller <c>adresse</c>.</summary>
    public required string ResultType { get; init; }

    /// <summary>
    /// For <c>adresse</c>: forældre-husnummer. For <c>husnummer</c>: samme som <see cref="LocalId"/>.
    /// </summary>
    public string? HusnummerId { get; init; }

    /// <summary>Opretter ids fra et autocomplete-resultat.</summary>
    public static DarAddressIds FromSelection(DanishAddressAutocompleteResult selection)
    {
        if (selection == null)
        {
            throw new ArgumentNullException(nameof(selection));
        }

        return new DarAddressIds
        {
            LocalId = selection.LocalId,
            ResultType = selection.ResultType,
            HusnummerId = selection.HusnummerId
        };
    }

    /// <summary>Genopbygger autocomplete-valg til <see cref="Services.Dar.DarAdresseopslagService.LookupFromAutocompleteAsync"/>.</summary>
    public DanishAddressAutocompleteResult ToSelection(string displayName = "") =>
        new(
            LocalId,
            displayName,
            string.Empty,
            string.Empty,
            string.Empty,
            IsCompleteAddress: true,
            ResultType: ResultType,
            HusnummerId: HusnummerId);

    /// <summary>DAR husnummer-id til opslag og BBR.</summary>
    public string GetHusnummerIdForLookup()
    {
        if (IsAdresse())
        {
            if (!string.IsNullOrWhiteSpace(HusnummerId))
            {
                return HusnummerId!;
            }

            throw new InvalidOperationException(
                "ResultType 'adresse' kræver HusnummerId. Brug ids fra autocomplete — ikke DisplayName.");
        }

        if (string.IsNullOrWhiteSpace(LocalId))
        {
            throw new InvalidOperationException("LocalId mangler.");
        }

        return LocalId;
    }

    /// <summary>DAR adresse-id for enhed; null for adgangsadresse.</summary>
    public string? GetAdresseLocalId() => IsAdresse() ? LocalId : null;

    private bool IsAdresse() =>
        string.Equals(ResultType, "adresse", StringComparison.OrdinalIgnoreCase);
}
