using System;
using System.Collections.Generic;
using System.Text.Json;
using TheNerdCollective.Integrations.Dar.Models;

namespace TheNerdCollective.Integrations.Dar.Services.Dar.Internal;

internal static class DagiKommuneJsonParser
{
    internal static KommuneDto? ParseKommuneinddeling(JsonElement root)
    {
        if (TryMapElement(root, out var direct))
        {
            return direct;
        }

        foreach (var candidate in WalkObjects(root))
        {
            if (TryMapElement(candidate, out var mapped))
            {
                return mapped;
            }
        }

        return null;
    }

    internal static IReadOnlyList<KommuneDto> ParseKommuneList(JsonElement root)
    {
        if (root.ValueKind == JsonValueKind.Array)
        {
            return ParseKommuneArray(root);
        }

        if (root.TryGetProperty("features", out var features) && features.ValueKind == JsonValueKind.Array)
        {
            return ParseGeoJsonFeatures(features);
        }

        var single = ParseKommuneinddeling(root);
        return single is null ? Array.Empty<KommuneDto>() : new[] { single };
    }

    private static IReadOnlyList<KommuneDto> ParseGeoJsonFeatures(JsonElement features)
    {
        var items = new List<KommuneDto>();
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

    private static IReadOnlyList<KommuneDto> ParseKommuneArray(JsonElement array)
    {
        var items = new List<KommuneDto>();
        foreach (var item in array.EnumerateArray())
        {
            if (TryMapElement(item, out var mapped))
            {
                items.Add(mapped);
            }
        }

        return items;
    }

    private static IEnumerable<JsonElement> WalkObjects(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object)
        {
            yield break;
        }

        foreach (var property in root.EnumerateObject())
        {
            if (property.NameEquals("Kommuneinddeling")
                || property.NameEquals("kommuneinddeling")
                || property.NameEquals("KommuneInddeling"))
            {
                yield return property.Value;
            }

            if (property.Value.ValueKind == JsonValueKind.Object)
            {
                yield return property.Value;
                foreach (var nested in WalkObjects(property.Value))
                {
                    yield return nested;
                }
            }
            else if (property.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in property.Value.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.Object)
                    {
                        yield return item;
                    }
                }
            }
        }
    }

    private static bool TryMapElement(JsonElement element, out KommuneDto kommune)
    {
        kommune = null!;

        if (element.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        var navn = ReadString(element, "navn", "Navn");
        var kommunekode = ReadString(element, "kommunekode", "Kommunekode");
        var idLokalId = ReadString(element, "id_lokalId", "idLokalId", "lokalId", "id");

        if (string.IsNullOrWhiteSpace(navn) && string.IsNullOrWhiteSpace(kommunekode))
        {
            return false;
        }

        kommune = new KommuneDto
        {
            IdLokalId = idLokalId,
            Navn = navn,
            Kommunekode = kommunekode
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
