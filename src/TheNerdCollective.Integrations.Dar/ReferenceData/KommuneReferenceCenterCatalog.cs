using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace TheNerdCollective.Integrations.Dar.ReferenceData;

/// <summary>
/// Dwarf/Fleming kommune-centre (embedded CSV) — bruges som repræsentativpunkt når DAWA er utilgængelig.
/// </summary>
internal static class KommuneReferenceCenterCatalog
{
    private const string EmbeddedResourceName =
        "TheNerdCollective.Integrations.Dar.ReferenceData.municipality-center-check.csv";

    private static readonly Lazy<IReadOnlyDictionary<string, (double Latitude, double Longitude)>> Centers =
        new(LoadCenters);

    internal static bool TryGetCenter(string? kommunekode, out double latitude, out double longitude)
    {
        latitude = 0;
        longitude = 0;
        if (string.IsNullOrWhiteSpace(kommunekode))
        {
            return false;
        }

        var key = kommunekode.Trim().PadLeft(4, '0');
        if (!Centers.Value.TryGetValue(key, out var center))
        {
            return false;
        }

        latitude = center.Latitude;
        longitude = center.Longitude;
        return true;
    }

    private static IReadOnlyDictionary<string, (double Latitude, double Longitude)> LoadCenters()
    {
        using var stream = OpenCsvStream();
        using var reader = new StreamReader(stream);
        var map = new Dictionary<string, (double, double)>(StringComparer.Ordinal);

        if (reader.ReadLine() is null)
        {
            return map;
        }

        while (reader.ReadLine() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var fields = SplitCsvLine(line);
            if (fields.Count < 4)
            {
                continue;
            }

            var code = fields[0].Trim().PadLeft(4, '0');
            if (!double.TryParse(fields[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude)
                || !double.TryParse(fields[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude))
            {
                continue;
            }

            map[code] = (latitude, longitude);
        }

        return map;
    }

    private static Stream OpenCsvStream()
    {
        var assembly = typeof(KommuneReferenceCenterCatalog).Assembly;
        var stream = assembly.GetManifestResourceStream(EmbeddedResourceName);
        if (stream is not null)
        {
            return stream;
        }

        var path = Path.Combine(AppContext.BaseDirectory, "ReferenceData", "municipality-center-check.csv");
        if (File.Exists(path))
        {
            return File.OpenRead(path);
        }

        return Stream.Null;
    }

    private static List<string> SplitCsvLine(string line)
    {
        var fields = new List<string>();
        var current = string.Empty;
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (c == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (c == ',' && !inQuotes)
            {
                fields.Add(current);
                current = string.Empty;
                continue;
            }

            current += c;
        }

        fields.Add(current);
        return fields;
    }
}
