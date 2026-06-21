using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TheNerdCollective.Integrations.Dar.Models;

/// <summary>
/// Hjælpere til at vælge og opslå det bedste autocomplete-resultat.
/// </summary>
public static class DanishAddressAutocompleteMatching
{
    private static readonly Regex UnitSearchPattern = new(
        @"\b\d+\s*(?:\.?\s*)?(?:sal|th|tv|mf|kl)\b|\b\d+(?:th|sal|tv)\b|\b(?:st|kl|mf)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    /// Vælger det bedste autocomplete-resultat til adresseopslag og KVHX.
    /// Foretrækker <c>adresse</c> når søgeteksten ligner en enhed (fx <c>2. tv</c>),
    /// ellers <c>husnummer</c> for adgangsadresse.
    /// </summary>
    public static DanishAddressAutocompleteResult? ResolveBestMatch(
        IEnumerable<DanishAddressAutocompleteResult> results,
        string? searchText = null)
    {
        var list = results?.ToList() ?? new List<DanishAddressAutocompleteResult>();
        if (list.Count == 0)
        {
            return null;
        }

        if (LooksLikeUnitSearch(searchText))
        {
            return list.FirstOrDefault(IsCompleteAdresse)
                ?? list.FirstOrDefault(r => r.IsCompleteAddress)
                ?? list.FirstOrDefault();
        }

        return list.FirstOrDefault(IsCompleteHusnummer)
            ?? list.FirstOrDefault(IsCompleteAdresse)
            ?? list.FirstOrDefault(r => r.IsCompleteAddress)
            ?? list.FirstOrDefault();
    }

    /// <summary>
    /// Returnerer DAR husnummer-id til <see cref="Services.Dar.DarAdresseopslagService.LookupByHusnummerIdAsync"/>.
    /// For type <c>adresse</c> bruges <see cref="DanishAddressAutocompleteResult.HusnummerId"/> — ikke <see cref="DanishAddressAutocompleteResult.LocalId"/>.
    /// </summary>
    public static string GetHusnummerIdForLookup(this DanishAddressAutocompleteResult selection)
    {
        if (selection == null)
        {
            throw new ArgumentNullException(nameof(selection));
        }

        if (IsAdresse(selection))
        {
            if (!string.IsNullOrWhiteSpace(selection.HusnummerId))
            {
                return selection.HusnummerId!;
            }

            throw new InvalidOperationException(
                "Autocomplete-resultat med type 'adresse' mangler HusnummerId. " +
                "Brug et komplet autocomplete-resultat — ikke DisplayName eller adresse-LocalId til opslag.");
        }

        if (string.IsNullOrWhiteSpace(selection.LocalId))
        {
            throw new InvalidOperationException("Autocomplete-resultat mangler LocalId.");
        }

        return selection.LocalId;
    }

    /// <summary>
    /// Returnerer adresse-id når <see cref="DanishAddressAutocompleteResult.ResultType"/> er <c>adresse</c>; ellers null.
    /// </summary>
    public static string? GetAdresseLocalId(this DanishAddressAutocompleteResult selection)
    {
        if (selection == null)
        {
            throw new ArgumentNullException(nameof(selection));
        }

        return IsAdresse(selection) ? selection.LocalId : null;
    }

    /// <summary>
    /// True når søgeteksten indeholder enhedsangivelse (fx <c>2. tv</c>, <c>1. sal</c>).
    /// </summary>
    public static bool LooksLikeUnitSearch(string? searchText) =>
        !string.IsNullOrWhiteSpace(searchText) && UnitSearchPattern.IsMatch(searchText);

    private static bool IsCompleteHusnummer(DanishAddressAutocompleteResult result) =>
        result.IsCompleteAddress && IsHusnummer(result);

    private static bool IsCompleteAdresse(DanishAddressAutocompleteResult result) =>
        result.IsCompleteAddress && IsAdresse(result);

    private static bool IsHusnummer(DanishAddressAutocompleteResult result) =>
        string.Equals(result.ResultType, "husnummer", StringComparison.OrdinalIgnoreCase);

    private static bool IsAdresse(DanishAddressAutocompleteResult result) =>
        string.Equals(result.ResultType, "adresse", StringComparison.OrdinalIgnoreCase);
}
