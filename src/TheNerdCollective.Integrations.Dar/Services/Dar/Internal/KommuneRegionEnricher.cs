using System;
using System.Collections.Generic;
using System.Linq;
using TheNerdCollective.Integrations.Dar.Mapping;
using TheNerdCollective.Integrations.Dar.Models;

namespace TheNerdCollective.Integrations.Dar.Services.Dar.Internal;

internal static class KommuneRegionEnricher
{
    internal static IReadOnlyList<KommuneDto> EnrichFromGraph(
        IReadOnlyList<KommuneGraphDto> graphKommuner,
        IReadOnlyList<RegionDto> regioner,
        IReadOnlyList<KommuneDto>? dawaKommuner = null)
    {
        var regionById = BuildRegionLookup(regioner);
        var dawaByCode = BuildKommuneLookup(dawaKommuner);

        return graphKommuner
            .Select(graph =>
            {
                var baseKommune = new KommuneDto
                {
                    IdLokalId = graph.IdLokalId,
                    Navn = graph.Navn,
                    Kommunekode = graph.Kommunekode
                };

                var enriched = ApplyRegion(
                    baseKommune,
                    TryResolveRegion(regionById, graph.RegionLokalid),
                    TryResolveDawa(dawaByCode, graph.Kommunekode));

                return ApplyRepresentativePoint(enriched, graph.Geometri);
            })
            .OrderBy(k => k.Navn, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    internal static IReadOnlyList<KommuneDto> EnrichExisting(
        IReadOnlyList<KommuneDto> kommuner,
        IReadOnlyList<RegionDto> regioner,
        IReadOnlyList<KommuneDto>? dawaKommuner = null)
    {
        if (AllHaveRegion(kommuner))
        {
            return kommuner;
        }

        var regionById = BuildRegionLookup(regioner);
        var dawaByCode = BuildKommuneLookup(dawaKommuner);

        return kommuner
            .Select(kommune => ApplyRegion(
                kommune,
                region: null,
                TryResolveDawa(dawaByCode, kommune.Kommunekode)))
            .OrderBy(k => k.Navn, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    private static KommuneDto ApplyRegion(KommuneDto kommune, RegionDto? region, KommuneDto? dawa)
    {
        if (!string.IsNullOrWhiteSpace(kommune.Regionskode) && !string.IsNullOrWhiteSpace(kommune.Regionnavn))
        {
            return kommune;
        }

        if (region is not null)
        {
            return kommune with
            {
                Regionskode = region.Regionskode,
                Regionnavn = region.Regionnavn
            };
        }

        if (dawa is not null
            && (!string.IsNullOrWhiteSpace(dawa.Regionskode) || !string.IsNullOrWhiteSpace(dawa.Regionnavn)))
        {
            return kommune with
            {
                Regionskode = dawa.Regionskode ?? kommune.Regionskode,
                Regionnavn = dawa.Regionnavn ?? kommune.Regionnavn
            };
        }

        return kommune;
    }

    private static bool AllHaveRegion(IReadOnlyList<KommuneDto> kommuner) =>
        kommuner.All(k =>
            !string.IsNullOrWhiteSpace(k.Regionskode) && !string.IsNullOrWhiteSpace(k.Regionnavn));

    private static Dictionary<string, RegionDto> BuildRegionLookup(IReadOnlyList<RegionDto> regioner)
    {
        var lookup = new Dictionary<string, RegionDto>(StringComparer.OrdinalIgnoreCase);

        foreach (var region in regioner)
        {
            var idLokalId = region.IdLokalId;
            if (!string.IsNullOrWhiteSpace(idLokalId))
            {
                lookup[idLokalId!] = region;
            }

            var regionskode = region.Regionskode;
            if (!string.IsNullOrWhiteSpace(regionskode))
            {
                lookup[regionskode!] = region;
            }
        }

        return lookup;
    }

    private static Dictionary<string, KommuneDto> BuildKommuneLookup(IReadOnlyList<KommuneDto>? kommuner)
    {
        if (kommuner is null || kommuner.Count == 0)
        {
            return new Dictionary<string, KommuneDto>(StringComparer.Ordinal);
        }

        return kommuner
            .Where(k => !string.IsNullOrWhiteSpace(k.Kommunekode))
            .GroupBy(k => k.Kommunekode!, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);
    }

    private static RegionDto? TryResolveRegion(IReadOnlyDictionary<string, RegionDto> regionById, string? regionLokalid)
    {
        var key = regionLokalid;
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        return regionById.TryGetValue(key!, out var region) ? region : null;
    }

    private static KommuneDto? TryResolveDawa(IReadOnlyDictionary<string, KommuneDto> dawaByCode, string? kommunekode)
    {
        var kommunekodeKey = kommunekode;
        if (string.IsNullOrWhiteSpace(kommunekodeKey))
        {
            return null;
        }

        return dawaByCode.TryGetValue(kommunekodeKey!, out var kommune) ? kommune : null;
    }

    private static KommuneDto ApplyRepresentativePoint(KommuneDto kommune, KoordinatDto? geometri)
    {
        var centroid = WktCentroidHelper.TryGetCentroidWgs84(geometri?.Wkt);
        if (centroid is null)
        {
            return kommune;
        }

        return kommune with
        {
            RepræsentativPunktLatitude = centroid.Value.Latitude,
            RepræsentativPunktLongitude = centroid.Value.Longitude
        };
    }
}
