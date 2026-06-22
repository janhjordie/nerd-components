using TheNerdCollective.Integrations.Dar.Mapping;
using TheNerdCollective.Integrations.Dar.Services;
using TheNerdCollective.Integrations.Dar.Services.Dar;
using Xunit;
using Xunit.Abstractions;

namespace TheNerdCollective.Integrations.Dar.IntegrationTests;

/// <summary>
/// Ugentlig readiness-probe: er DAGI GraphQL klar til at erstatte DAWA-fallback for geografi?
/// Testen fejler aldrig CI — den rapporterer via output og stdout.
/// </summary>
public sealed class DarDagiReadinessProbeTests
{
    private readonly ITestOutputHelper _output;

    public DarDagiReadinessProbeTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public async Task Probe_rapporterer_dagi_readiness()
    {
        var report = new List<string> { $"# DAGI readiness — {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC" };
        var checks = new List<bool>();

        try
        {
            var datafordelerOnly = IntegrationTestEnvironment.CreateDatafordelerOnlyServices();

            var kommuneListOk = false;
            var kommuneCount = 0;
            try
            {
                var kommuner = await datafordelerOnly.Dar.Kommune.GetAllAsync();
                kommuneCount = kommuner.Count;
                kommuneListOk = kommuneCount >= 90;
            }
            catch (InvalidOperationException)
            {
                kommuneListOk = false;
            }

            checks.Add(kommuneListOk);
            report.Add($"- Kommune-liste (GraphQL/WFS/REST, uden DAWA): {(kommuneListOk ? "OK" : "MANGLER")} ({kommuneCount} kommuner)");

            var polygonWkt = GeoCircleHelper.CreateCirclePolygonWkt(12.48168066, 56.04907246, 10000);
            var kommuneService = (DarKommuneService)datafordelerOnly.Dar.Kommune;
            var kommunerInCircle = await kommuneService
                .FindKommunerByGeometryAsync(polygonWkt, includeWfsFallback: false);
            var geometryOk = kommunerInCircle.Count > 0;
            checks.Add(geometryOk);
            report.Add($"- Spatial kommune (intersects cirkel): {(geometryOk ? "OK" : "MANGLER")} ({kommunerInCircle.Count} kommuner)");

            var coordinatesOk = false;
            try
            {
                var kommune = await datafordelerOnly.Dar.Kommune.FindByCoordinatesDatafordelerAsync(
                    56.02304649121056,
                    12.598664281911976);
                coordinatesOk = kommune.Kommunekode == "0217"
                    && kommune.RepræsentativPunktLatitude is not null;
            }
            catch (InvalidOperationException)
            {
                coordinatesOk = false;
            }

            checks.Add(coordinatesOk);
            report.Add($"- Punkt → kommune + repræsentativt punkt (0217): {(coordinatesOk ? "OK" : "MANGLER")}");

            var postnumre = await datafordelerOnly.Dar.Postnummer.GetByCircleAsync(
                12.48168066,
                56.04907246,
                10000);
            var circleOk = postnumre.Any(p => p.Postnummer == "3000")
                && postnumre.Any(p => p.Postnummer == "3050");
            checks.Add(circleOk);
            report.Add($"- Cirkel → postnumre (3000/3050, uden DAWA): {(circleOk ? "OK" : "MANGLER")} ({postnumre.Count} postnumre)");
            report.Add("- Kommune 0217 postnumre m. alle kommuner: (springes over i probe — REST-only sti er langsom; tjek manuelt når øvrige checks er grønne)");
        }
        catch (Exception ex) when (ex.Message.Contains("DAF-AUTH-0005", StringComparison.Ordinal))
        {
            report.Add("- Probe afbrudt: IP ikke whitelisted (DAF-AUTH-0005)");
            checks.Add(false);
        }

        var ready = checks.Count > 0 && checks.All(c => c);
        report.Add(string.Empty);
        report.Add($"READY_TO_DISABLE_DAWA_FALLBACK={(ready ? "true" : "false")}");
        if (ready)
        {
            report.Add("Anbefaling: Planlæg fjernelse af EnableDawaFallback / EnableDawaEnrichment og opdater README.");
        }
        else
        {
            report.Add("Anbefaling: Behold DAWA-fallback. Tjek docs/monitoring/DATAFORDELER-DAWA-WATCH.md for officielle kilder.");
        }

        var text = string.Join(Environment.NewLine, report);
        _output.WriteLine(text);
        Console.WriteLine(text);
    }
}
