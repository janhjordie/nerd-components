using TheNerdCollective.Integrations.Dar.IntegrationTests.Migration;
using Xunit;
using Xunit.Abstractions;

namespace TheNerdCollective.Integrations.Dar.IntegrationTests;

/// <summary>
/// Kort readiness-probe (bagudkompatibel) — se <see cref="DarDawaMigrationProbeTests"/> for fuld rapport.
/// </summary>
public sealed class DarDagiReadinessProbeTests
{
    private readonly ITestOutputHelper _output;

    public DarDagiReadinessProbeTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public async Task Probe_rapporterer_dagi_readiness()
    {
        var rows = await DarDawaMigrationReporter.RunAllProbesAsync(
            IntegrationTestEnvironment.CreateDatafordelerOnlyServices(),
            IntegrationTestEnvironment.CreateServices());

        var report = DarDawaMigrationReporter.FormatReport(rows);
        _output.WriteLine(report);
        Console.WriteLine(report);
    }
}
