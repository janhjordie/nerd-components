using System.Text;
using TheNerdCollective.Integrations.Dar.GraphQL;
using TheNerdCollective.Integrations.Dar.Mapping;
using TheNerdCollective.Integrations.Dar.Services;
using TheNerdCollective.Integrations.Dar.Services.Dar;

namespace TheNerdCollective.Integrations.Dar.IntegrationTests.Migration;

/// <summary>
/// Sammenligner opslag med og uden DAWA-fallback for at afgøre migrationsstatus.
/// </summary>
internal static class DarDawaMigrationReporter
{
    internal const double KbhLatitude = 55.6759;
    internal const double KbhLongitude = 12.5655;
    internal const double HelsingoerLatitude = 56.02304649121056;
    internal const double HelsingoerLongitude = 12.598664281911976;
    internal const double CircleLongitude = 12.48168066;
    internal const double CircleLatitude = 56.04907246;
    internal const int CircleRadiusMeters = 10000;

    internal static async Task<IReadOnlyList<MigrationProbeRow>> RunAllProbesAsync(
        DarServices datafordelerOnly,
        DarServices withDawa)
    {
        var rows = new List<MigrationProbeRow>
        {
            await ProbeKommuneGetAllAsync(datafordelerOnly, withDawa),
            await ProbeKommuneFindByCoordinatesAsync(datafordelerOnly, withDawa),
            await ProbeKommuneFindByCoordinatesDatafordelerAsync(datafordelerOnly, withDawa),
            await ProbeKommuneFindByGeometryAsync(datafordelerOnly, withDawa),
            await ProbeRegionGetAllAsync(datafordelerOnly, withDawa),
            ProbePostnummerGetAllActive(),
            await ProbePostnummerByMunicipalityAsync(datafordelerOnly, withDawa),
            await ProbePostnummerByPostalCodeAsync(datafordelerOnly, withDawa),
            await ProbePostnummerByCircleAsync(datafordelerOnly, withDawa),
            await ProbePostnummerByMunicipalityWithKommunerAsync(datafordelerOnly, withDawa)
        };

        return rows;
    }

    internal static string FormatReport(IReadOnlyList<MigrationProbeRow> rows)
    {
        var sb = new StringBuilder();
        var utcNow = DateTime.UtcNow;
        var blockedRows = rows.Where(r => r.DatafordelerBlocked).ToList();
        var conclusiveRows = rows.Where(r => !r.DatafordelerBlocked && r.InferredSource != MigrationDataSource.NotApplicable).ToList();
        var ready = blockedRows.Count == 0
            && conclusiveRows.Count > 0
            && conclusiveRows.All(r => r.DatafordelerOk);

        sb.AppendLine("================================================================================");
        sb.AppendLine("  Datafordeler migration / DAWA fallback rapport");
        sb.AppendLine($"  {utcNow:yyyy-MM-dd HH:mm} UTC");
        sb.AppendLine("================================================================================");
        sb.AppendLine();

        if (blockedRows.Count > 0)
        {
            sb.AppendLine("Advarsel: Datafordeler kunne ikke testes (DAF-AUTH-0005) — kør fra whitelisted IP for gyldig konklusion.");
            sb.AppendLine();
        }

        var dawaNeeded = rows.Any(r => r.FallbackRequired);
        if (blockedRows.Count > 0 && !dawaNeeded)
        {
            sb.AppendLine("Status: UKLAR (Datafordeler blokeret — migration ikke afgjort)");
        }
        else if (dawaNeeded)
        {
            sb.AppendLine("Status: DAWA_FALLBACK_STADIG_NOEDVENDIG");
        }
        else if (ready)
        {
            sb.AppendLine("Status: DATAFORDELER_KLAR — DAWA-fallback kan overvejes fjernet");
        }
        else
        {
            sb.AppendLine("Status: DATAFORDELER_MANGLER_DATA (uden DAWA)");
        }

        sb.AppendLine($"READY_TO_DISABLE_DAWA_FALLBACK={(ready ? "true" : "false")}");
        sb.AppendLine();

        sb.AppendLine("Opslag                              | Kun Datafordeler     | Med DAWA (default)   | Kilde nu        | Fallback?");
        sb.AppendLine("------------------------------------|----------------------|----------------------|-----------------|----------");

        foreach (var row in rows)
        {
            sb.AppendLine(
                $"{Pad(row.Operation, 35)} | {Pad(row.DatafordelerDetail, 20)} | {Pad(row.WithDawaDetail, 20)} | {Pad(SourceLabel(row.InferredSource), 15)} | {(row.FallbackRequired ? "JA" : "NEJ")}");
        }

        sb.AppendLine();
        sb.AppendLine("Forklaring:");
        sb.AppendLine("  Datafordeler  — opslag lykkedes uden DAWA (GraphQL / REST DAGI / WFS / DAR)");
        sb.AppendLine("  DAWA          — kun default med EnableDawaFallback/EnableDawaEnrichment gav data");
        sb.AppendLine("  N/A           — metoden bruger ikke DAWA (kun DAR GraphQL)");
        sb.AppendLine("  Blokeret      — Datafordeler afviste kald (typisk IP ikke whitelisted)");
        sb.AppendLine();

        if (blockedRows.Count > 0)
        {
            sb.AppendLine("Datafordeler ikke testet (auth) for:");
            foreach (var row in blockedRows)
            {
                sb.AppendLine($"  - {row.Operation}");
            }

            sb.AppendLine();
        }

        var stillNeeded = rows.Where(r => r.FallbackRequired).Select(r => r.Operation).ToList();
        if (stillNeeded.Count > 0)
        {
            sb.AppendLine("DAWA-fallback stadig nødvendig for:");
            foreach (var op in stillNeeded)
            {
                sb.AppendLine($"  - {op}");
            }

            sb.AppendLine();
            sb.AppendLine("Behold i config:");
            sb.AppendLine("  Dagi.EnableDawaFallback = true");
            sb.AppendLine("  Postnummer.EnableDawaEnrichment = true");
        }
        else if (ready)
        {
            sb.AppendLine("Alle DAWA-afhængige opslag virker uden DAWA.");
            sb.AppendLine("Overvej at sætte EnableDawaFallback og EnableDawaEnrichment til false efter manuel verifikation.");
        }
        else if (blockedRows.Count == 0)
        {
            sb.AppendLine("Ingen DAWA-fallback nødvendig ifølge probe — men Datafordeler returnerede ikke data for alle opslag.");
        }

        sb.AppendLine();
        sb.AppendLine("================================================================================");
        return sb.ToString();
    }

    internal static void WriteReportIfConfigured(string report)
    {
        var path = Environment.GetEnvironmentVariable("DAR_MIGRATION_REPORT_PATH");
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, report, Encoding.UTF8);
    }

    private static async Task<MigrationProbeRow> ProbeKommuneGetAllAsync(
        DarServices datafordelerOnly,
        DarServices withDawa)
    {
        var df = await TryAsync(() => datafordelerOnly.Dar.Kommune.GetAllAsync());
        var dawa = await TryAsync(() => withDawa.Dar.Kommune.GetAllAsync());

        var dfOk = df.Success && df.Value!.Count >= 90;
        var dawaOk = dawa.Success && dawa.Value!.Count >= 90;

        return BuildRow(
            "Kommune.GetAllAsync",
            df,
            dfOk,
            dfOk ? $"{df.Value!.Count} kommuner" : Detail(df),
            dawa,
            dawaOk,
            dawaOk ? $"{dawa.Value!.Count} kommuner" : Detail(dawa),
            requiresFallback: true);
    }

    private static async Task<MigrationProbeRow> ProbeKommuneFindByCoordinatesAsync(
        DarServices datafordelerOnly,
        DarServices withDawa)
    {
        var df = await TryAsync(() => datafordelerOnly.Dar.Kommune.FindByCoordinatesAsync(KbhLatitude, KbhLongitude));
        var dawa = await TryAsync(() => withDawa.Dar.Kommune.FindByCoordinatesAsync(KbhLatitude, KbhLongitude));

        var dfOk = df.Success && df.Value!.Kommunekode == "0101";
        var dawaOk = dawa.Success && dawa.Value!.Kommunekode == "0101";

        return BuildRow(
            "Kommune.FindByCoordinatesAsync",
            df,
            dfOk,
            dfOk ? $"{df.Value!.Navn} ({df.Value.Kommunekode})" : Detail(df),
            dawa,
            dawaOk,
            dawaOk ? $"{dawa.Value!.Navn} ({dawa.Value.Kommunekode})" : Detail(dawa),
            requiresFallback: true);
    }

    private static async Task<MigrationProbeRow> ProbeKommuneFindByCoordinatesDatafordelerAsync(
        DarServices datafordelerOnly,
        DarServices withDawa)
    {
        var df = await TryAsync(() => datafordelerOnly.Dar.Kommune.FindByCoordinatesDatafordelerAsync(
            HelsingoerLatitude,
            HelsingoerLongitude));
        var dawa = await TryAsync(() => withDawa.Dar.Kommune.FindByCoordinatesDatafordelerAsync(
            HelsingoerLatitude,
            HelsingoerLongitude));

        var dfOk = df.Success
            && df.Value!.Kommunekode == "0217"
            && df.Value.RepræsentativPunktLatitude is not null;
        var dawaOk = dawa.Success
            && dawa.Value!.Kommunekode == "0217"
            && dawa.Value.RepræsentativPunktLatitude is not null;

        return BuildRow(
            "Kommune.FindByCoordinatesDatafordelerAsync",
            df,
            dfOk,
            dfOk ? "0217 + repr. punkt" : Detail(df),
            dawa,
            dawaOk,
            dawaOk ? "0217 + repr. punkt" : Detail(dawa),
            requiresFallback: false);
    }

    private static async Task<MigrationProbeRow> ProbeKommuneFindByGeometryAsync(
        DarServices datafordelerOnly,
        DarServices withDawa)
    {
        var polygonWkt = GeoCircleHelper.CreateCirclePolygonWkt(
            CircleLongitude,
            CircleLatitude,
            CircleRadiusMeters);

        var dfKommune = (DarKommuneService)datafordelerOnly.Dar.Kommune;
        var dawaKommune = (DarKommuneService)withDawa.Dar.Kommune;

        var df = await TryAsync(() => dfKommune.FindKommunerByGeometryAsync(polygonWkt, includeWfsFallback: false));
        var dawa = await TryAsync(() => dawaKommune.FindKommunerByGeometryAsync(polygonWkt, includeWfsFallback: true));

        var dfOk = df.Success && df.Value!.Count > 0;
        var dawaOk = dawa.Success && dawa.Value!.Count > 0;

        return BuildRow(
            "Kommune.FindKommunerByGeometryAsync",
            df,
            dfOk,
            dfOk ? $"{df.Value!.Count} kommuner" : Detail(df),
            dawa,
            dawaOk,
            dawaOk ? $"{dawa.Value!.Count} kommuner" : Detail(dawa),
            requiresFallback: true);
    }

    private static async Task<MigrationProbeRow> ProbeRegionGetAllAsync(
        DarServices datafordelerOnly,
        DarServices withDawa)
    {
        var df = await TryAsync(() => datafordelerOnly.Dar.Region.GetAllAsync());
        var dawa = await TryAsync(() => withDawa.Dar.Region.GetAllAsync());

        var dfOk = df.Success && df.Value!.Count >= 5;
        var dawaOk = dawa.Success && dawa.Value!.Count >= 5;

        return BuildRow(
            "Region.GetAllAsync",
            df,
            dfOk,
            dfOk ? $"{df.Value!.Count} regioner" : Detail(df),
            dawa,
            dawaOk,
            dawaOk ? $"{dawa.Value!.Count} regioner" : Detail(dawa),
            requiresFallback: true);
    }

    private static MigrationProbeRow ProbePostnummerGetAllActive()
    {
        return new MigrationProbeRow(
            "Postnummer.GetAllActiveAsync",
            DatafordelerOk: true,
            DatafordelerBlocked: false,
            DatafordelerDetail: "DAR GraphQL (ingen DAWA-sti)",
            WithDawaOk: true,
            WithDawaDetail: "DAR GraphQL (ingen DAWA-sti)",
            InferredSource: MigrationDataSource.NotApplicable,
            FallbackRequired: false);
    }

    private static async Task<MigrationProbeRow> ProbePostnummerByMunicipalityAsync(
        DarServices datafordelerOnly,
        DarServices withDawa)
    {
        var df = await TryAsync(() => datafordelerOnly.Dar.Postnummer.GetByMunicipalityCodeAsync("0101"));
        var dawa = await TryAsync(() => withDawa.Dar.Postnummer.GetByMunicipalityCodeAsync("0101"));

        var dfOk = df.Success && df.Value!.Count > 0;
        var dawaOk = dawa.Success && dawa.Value!.Count > 0;

        return BuildRow(
            "Postnummer.GetByMunicipalityCodeAsync",
            df,
            dfOk,
            dfOk ? $"{df.Value!.Count} postnumre" : Detail(df),
            dawa,
            dawaOk,
            dawaOk ? $"{dawa.Value!.Count} postnumre" : Detail(dawa),
            requiresFallback: true);
    }

    private static async Task<MigrationProbeRow> ProbePostnummerByPostalCodeAsync(
        DarServices datafordelerOnly,
        DarServices withDawa)
    {
        var df = await TryAsync(() => datafordelerOnly.Dar.Postnummer.GetByPostalCodesAsync(new[] { "2100" }));
        var dawa = await TryAsync(() => withDawa.Dar.Postnummer.GetByPostalCodesAsync(new[] { "2100" }));

        var dfOk = df.Success && df.Value!.Any(p => p.Postnummer == "2100" && !string.IsNullOrWhiteSpace(p.Kommunekode));
        var dawaOk = dawa.Success && dawa.Value!.Any(p => p.Postnummer == "2100" && !string.IsNullOrWhiteSpace(p.Kommunekode));

        return BuildRow(
            "Postnummer.GetByPostalCodesAsync",
            df,
            dfOk,
            dfOk ? "2100 + kommune" : Detail(df),
            dawa,
            dawaOk,
            dawaOk ? "2100 + kommune" : Detail(dawa),
            requiresFallback: true);
    }

    private static async Task<MigrationProbeRow> ProbePostnummerByCircleAsync(
        DarServices datafordelerOnly,
        DarServices withDawa)
    {
        var df = await TryAsync(() => datafordelerOnly.Dar.Postnummer.GetByCircleAsync(
            CircleLongitude,
            CircleLatitude,
            CircleRadiusMeters));
        var dawa = await TryAsync(() => withDawa.Dar.Postnummer.GetByCircleAsync(
            CircleLongitude,
            CircleLatitude,
            CircleRadiusMeters));

        var dfOk = df.Success
            && df.Value!.Any(p => p.Postnummer == "3000")
            && df.Value.Any(p => p.Postnummer == "3050");
        var dawaOk = dawa.Success
            && dawa.Value!.Any(p => p.Postnummer == "3000")
            && dawa.Value.Any(p => p.Postnummer == "3050");

        return BuildRow(
            "Postnummer.GetByCircleAsync",
            df,
            dfOk,
            dfOk ? $"{df.Value!.Count} (3000+3050)" : Detail(df),
            dawa,
            dawaOk,
            dawaOk ? $"{dawa.Value!.Count} (3000+3050)" : Detail(dawa),
            requiresFallback: true);
    }

    private static async Task<MigrationProbeRow> ProbePostnummerByMunicipalityWithKommunerAsync(
        DarServices datafordelerOnly,
        DarServices withDawa)
    {
        var df = await TryAsync(() => datafordelerOnly.Dar.Postnummer.GetByMunicipalityCodeWithKommunerAsync("0217"));
        var dawa = await TryAsync(() => withDawa.Dar.Postnummer.GetByMunicipalityCodeWithKommunerAsync("0217"));

        var dfOk = df.Success && df.Value!.Count > 0 && df.Value.All(p => p.Kommuner.Count > 0);
        var dawaOk = dawa.Success && dawa.Value!.Count > 0 && dawa.Value.All(p => p.Kommuner.Count > 0);

        return BuildRow(
            "Postnummer.GetByMunicipalityCodeWithKommunerAsync",
            df,
            dfOk,
            dfOk ? $"{df.Value!.Count} postnumre" : Detail(df),
            dawa,
            dawaOk,
            dawaOk ? $"{dawa.Value!.Count} postnumre" : Detail(dawa),
            requiresFallback: true);
    }

    private static MigrationProbeRow BuildRow<T>(
        string operation,
        TryResult<T> df,
        bool dfOk,
        string dfDetail,
        TryResult<T> dawa,
        bool dawaOk,
        string dawaDetail,
        bool requiresFallback)
    {
        var dfBlocked = df.IsBlocked;
        var source = InferSource(dfOk, dawaOk, dfBlocked, requiresFallback);
        var fallbackRequired = requiresFallback && source == MigrationDataSource.DawaFallback;

        return new MigrationProbeRow(
            operation,
            dfOk,
            dfBlocked,
            dfDetail,
            dawaOk,
            dawaDetail,
            source,
            fallbackRequired);
    }

    private static MigrationDataSource InferSource(bool dfOk, bool dawaOk, bool dfBlocked, bool hasDawaPath)
    {
        if (!hasDawaPath)
        {
            return dfBlocked ? MigrationDataSource.Blocked : MigrationDataSource.NotApplicable;
        }

        if (dfBlocked)
        {
            return MigrationDataSource.Blocked;
        }

        if (dfOk)
        {
            return MigrationDataSource.Datafordeler;
        }

        if (dawaOk)
        {
            return MigrationDataSource.DawaFallback;
        }

        return MigrationDataSource.Unavailable;
    }

    private static string SourceLabel(MigrationDataSource source) => source switch
    {
        MigrationDataSource.Datafordeler => "Datafordeler",
        MigrationDataSource.DawaFallback => "DAWA",
        MigrationDataSource.Unavailable => "Utilgængelig",
        MigrationDataSource.Blocked => "Blokeret",
        MigrationDataSource.NotApplicable => "N/A",
        _ => source.ToString()
    };

    private static string Pad(string value, int width)
    {
        if (value.Length >= width)
        {
            return value[..Math.Min(width, value.Length)];
        }

        return value.PadRight(width);
    }

    private static string Detail<T>(TryResult<T> result)
    {
        if (result.IsBlocked)
        {
            return "DAF-AUTH-0005";
        }

        if (!result.Success)
        {
            var message = result.ErrorMessage ?? "fejl";
            if (message.Length > 18)
            {
                message = message[..15] + "...";
            }

            return message;
        }

        return "tomt/ugyldigt";
    }

    private static async Task<TryResult<T>> TryAsync<T>(Func<Task<T>> action)
    {
        try
        {
            var value = await action().ConfigureAwait(false);
            return TryResult<T>.Ok(value);
        }
        catch (DatafordelerApiException ex) when (ex.ResponseBody.Contains("DAF-AUTH-0005", StringComparison.Ordinal))
        {
            return TryResult<T>.BlockedResult();
        }
        catch (Exception ex)
        {
            return TryResult<T>.Fail(ex.Message);
        }
    }

    private sealed class TryResult<T>
    {
        public bool Success { get; private init; }

        public bool IsBlocked { get; private init; }

        public T? Value { get; private init; }

        public string? ErrorMessage { get; private init; }

        public static TryResult<T> Ok(T value) => new() { Success = true, Value = value };

        public static TryResult<T> Fail(string message) => new() { Success = false, ErrorMessage = message };

        public static TryResult<T> BlockedResult() => new() { IsBlocked = true };
    }
}
