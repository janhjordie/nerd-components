using TheNerdCollective.Integrations.Dar.IntegrationTests.Migration;
using Xunit;
using Xunit.Abstractions;

namespace TheNerdCollective.Integrations.Dar.IntegrationTests;

/// <summary>
/// Sammenligner alle DAWA-afhængige opslag med og uden fallback.
/// Fejler aldrig — skriver læsbar rapport til stdout og valgfri fil.
/// </summary>
public sealed class DarDawaMigrationProbeTests
{
    private readonly ITestOutputHelper _output;

    public DarDawaMigrationProbeTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public async Task Migration_probe_rapporterer_datafordeler_vs_dawa()
    {
        var datafordelerOnly = IntegrationTestEnvironment.CreateDatafordelerOnlyServices();
        var withDawa = IntegrationTestEnvironment.CreateServices();
        var rows = await DarDawaMigrationReporter.RunAllProbesAsync(datafordelerOnly, withDawa);

        var report = DarDawaMigrationReporter.FormatReport(rows);
        DarDawaMigrationReporter.WriteReportIfConfigured(report);

        _output.WriteLine(report);
        Console.WriteLine(report);
    }
}
