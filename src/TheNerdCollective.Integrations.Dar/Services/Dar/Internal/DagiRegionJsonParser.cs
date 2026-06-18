using System;
using System.Collections.Generic;
using System.Text.Json;
using TheNerdCollective.Integrations.Dar.Models;

namespace TheNerdCollective.Integrations.Dar.Services.Dar.Internal;

internal static class DagiRegionJsonParser
{
    internal static IReadOnlyList<RegionDto> ParseRegionList(JsonElement root)
    {
        if (root.ValueKind == JsonValueKind.Array)
        {
            return ParseRegionArray(root);
        }

        if (root.TryGetProperty("features", out var features) && features.ValueKind == JsonValueKind.Array)
        {
            var items = new List<RegionDto>();
            foreach (var feature in features.EnumerateArray())
            {
                if (feature.TryGetProperty("properties", out var properties)
                    && TryMapElement(properties, out var mapped))
                {
                    items.Add(mapped);
                }
            }

            return items;
        }

        return Array.Empty<RegionDto>();
    }

    private static IReadOnlyList<RegionDto> ParseRegionArray(JsonElement array)
    {
        var items = new List<RegionDto>();
        foreach (var item in array.EnumerateArray())
        {
            if (TryMapElement(item, out var mapped))
            {
                items.Add(mapped);
            }
        }

        return items;
    }

    private static bool TryMapElement(JsonElement element, out RegionDto region)
    {
        region = null!;

        if (element.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        var navn = ReadString(element, "navn", "Navn");
        var regionskode = ReadString(element, "regionskode", "Regionskode", "kode", "Kode");
        var idLokalId = ReadString(element, "id_lokalId", "idLokalId", "lokalId", "id", "dagi_id");

        if (string.IsNullOrWhiteSpace(navn) && string.IsNullOrWhiteSpace(regionskode))
        {
            return false;
        }

        region = new RegionDto
        {
            IdLokalId = idLokalId,
            Regionnavn = navn,
            Regionskode = regionskode
        };

        return true;
    }

    private static string? ReadString(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (!element.TryGetProperty(name, out var value))
            {
                continue;
            }

            if (value.ValueKind == JsonValueKind.String)
            {
                return value.GetString();
            }

            if (value.ValueKind is JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False)
            {
                return value.ToString();
            }
        }

        return null;
    }
}
