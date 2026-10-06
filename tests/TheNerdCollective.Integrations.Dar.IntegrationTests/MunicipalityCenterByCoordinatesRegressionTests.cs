using System.Globalization;
using System.Text;
using TheNerdCollective.Integrations.Dar.GraphQL;
using TheNerdCollective.Integrations.Dar.Models;
using TheNerdCollective.Integrations.Dar.Services;
using Xunit;

namespace TheNerdCollective.Integrations.Dar.IntegrationTests;

/// <summary>
/// Regression for Dwarf municipality-center matrix: each kommune's representative point
/// should resolve back to the same <see cref="KommuneDto.Kommunekode"/>.
/// </summary>
public sealed class MunicipalityCenterByCoordinatesRegressionTests
{
    [SkippableFact]
    public async Task FindByCoordinatesAsync_returnerer_samme_kommunekode_ved_repraesentativt_punkt()
    {
        DarServices services;
        try
        {
            services = IntegrationTestEnvironment.CreateServices();
        }
        catch (DatafordelerApiException ex) when (ex.ResponseBody.Contains("DAF-AUTH-0005", StringComparison.Ordinal))
        {
            Skip.If(true, "IP not whitelisted in Datafordeler (DAF-AUTH-0005).");
            return;
        }
        IReadOnlyList<KommuneDto> kommuner;
        try
        {
            kommuner = await services.Dar.Kommune.GetAllAsync();
        }
        catch (InvalidOperationException ex)
        {
            Skip.If(true, ex.Message);
            return;
        }

        Skip.If(kommuner.Count < 90, "DAGI kommune-liste utilgængelig fra dette netværk.");

        var failures = new List<string>();
        foreach (var kommune in kommuner)
        {
            if (kommune.RepræsentativPunktLatitude is null || kommune.RepræsentativPunktLongitude is null)
            {
                continue;
            }

            try
            {
                var resolved = await services.Dar.Kommune.FindByCoordinatesAsync(
                    kommune.RepræsentativPunktLatitude.Value,
                    kommune.RepræsentativPunktLongitude.Value);

                if (!string.Equals(resolved.Kommunekode, kommune.Kommunekode, StringComparison.Ordinal))
                {
                    failures.Add(
                        $"{kommune.Kommunekode} {kommune.Navn}: forventet {kommune.Kommunekode}, fik {resolved.Kommunekode} {resolved.Navn} " +
                        $"@ ({kommune.RepræsentativPunktLatitude.Value.ToString(CultureInfo.InvariantCulture)}, " +
                        $"{kommune.RepræsentativPunktLongitude.Value.ToString(CultureInfo.InvariantCulture)})");
                }
            }
            catch (InvalidOperationException)
            {
                failures.Add(
                    $"{kommune.Kommunekode} {kommune.Navn}: ingen kommune ved repræsentativt punkt " +
                    $"({kommune.RepræsentativPunktLatitude.Value.ToString(CultureInfo.InvariantCulture)}, " +
                    $"{kommune.RepræsentativPunktLongitude.Value.ToString(CultureInfo.InvariantCulture)})");
            }
        }

        if (failures.Count > 0)
        {
            var message = new StringBuilder();
            message.AppendLine($"Kommune center regression: {failures.Count} fejl.");
            foreach (var line in failures.Take(25))
            {
                message.AppendLine(line);
            }

            if (failures.Count > 25)
            {
                message.AppendLine($"... og {failures.Count - 25} flere.");
            }

            Assert.Fail(message.ToString());
        }
    }
}
