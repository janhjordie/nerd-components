using TheNerdCollective.Integrations.Dar.Models;
using TheNerdCollective.Integrations.Dar.TestWeb.Models;

namespace TheNerdCollective.Integrations.Dar.TestWeb.Services;

internal static class DarLookupMethodCalls
{
    internal static LookupMethodCalls Build(DarLookupRequest request, DarFullLookupResult result)
    {
        var street = MethodCallFormatter.CsString(request.StreetAndNumber);
        var postal = MethodCallFormatter.CsString(request.PostalCode);
        var city = MethodCallFormatter.CsNullableString(request.City);
        var husnummerId = MethodCallFormatter.CsString(result.Adresseopslag?.HusnummerId);

        var adresseopslag =
            "var adresseopslag = await services.Dar.Adresseopslag.LookupAsync(\n" +
            $"    {street},\n" +
            $"    {postal},\n" +
            $"    {city});";

        var husnummer =
            "var husnummer = await services.Dar.Husnummer.FindByAddressAsync(\n" +
            $"    {street},\n" +
            $"    {postal});";

        var bygning =
            "var bygninger = await services.Bbr.Bygning.GetAllByHusnummerIdAsync(\n" +
            $"    {husnummerId});";

        var firstBygningId = result.Bygninger.FirstOrDefault()?.IdLokalId;
        var bygningIdLiteral = MethodCallFormatter.CsString(firstBygningId);

        var enhed = result.Bygninger.Count <= 1
            ? $"var enheder = await services.Bbr.Enhed.GetByBygningIdAsync({bygningIdLiteral});"
            : "// Gentages for hver bygning\n" +
              $"var enheder = await services.Bbr.Enhed.GetByBygningIdAsync({bygningIdLiteral});";

        var etage = result.Bygninger.Count <= 1
            ? $"var etager = await services.Bbr.Etage.GetByBygningIdAsync({bygningIdLiteral});"
            : "// Gentages for hver bygning\n" +
              $"var etager = await services.Bbr.Etage.GetByBygningIdAsync({bygningIdLiteral});";

        var opgang = result.Bygninger.Count <= 1
            ? $"var opgange = await services.Bbr.Opgang.GetByBygningIdAsync({bygningIdLiteral});"
            : "// Gentages for hver bygning\n" +
              $"var opgange = await services.Bbr.Opgang.GetByBygningIdAsync({bygningIdLiteral});";

        var tekniskAnlaeg = result.Bygninger.Count <= 1
            ? $"var tekniskeAnlaeg = await services.Bbr.TekniskAnlaeg.GetByBygningIdAsync({bygningIdLiteral});"
            : "// Gentages for hver bygning\n" +
              $"var tekniskeAnlaeg = await services.Bbr.TekniskAnlaeg.GetByBygningIdAsync({bygningIdLiteral});";

        var grundId = result.Grund?.IdLokalId ?? result.Bygninger.FirstOrDefault(b => !string.IsNullOrWhiteSpace(b.Grund))?.Grund;
        var grundIdLiteral = MethodCallFormatter.CsString(grundId);

        var grund =
            $"var grund = await services.Bbr.Grund.GetByIdAsync({grundIdLiteral});";

        var jordstykker =
            $"var jordstykker = await services.Bbr.Grund.GetJordstykkerByGrundIdAsync({grundIdLiteral});";

        var bygningEjendomsrelation = result.Bygninger.Count <= 1
            ? $"var bygningEjendomsrelationer = await services.Bbr.Ejendomsrelation.GetByBygningIdAsync({bygningIdLiteral});"
            : "// Gentages for hver bygning\n" +
              $"var bygningEjendomsrelationer = await services.Bbr.Ejendomsrelation.GetByBygningIdAsync({bygningIdLiteral});";

        var ejendomsrelation =
            "var ejendomsrelationer = await services.Bbr.Ejendomsrelation.ResolveAsync(\n" +
            "    bygningEjendomsrelationer,\n" +
            "    grund);";

        return new LookupMethodCalls
        {
            Overview =
                "var services = DarClientFactory.Create(darOptions, httpClient);\n\n" +
                adresseopslag + "\n\n" +
                husnummer + "\n\n" +
                bygning,
            AdresseopslagDar = adresseopslag + "\n\n// Native DAR: adresseopslag.Dar",
            Adresseopslag = adresseopslag,
            KvHxInput = adresseopslag + "\n\n// Legacy: adresseopslag.KvHxInput",
            Husnummer = husnummer,
            Bygning = bygning,
            Enhed = enhed,
            Etage = etage,
            Opgang = opgang,
            TekniskAnlaeg = tekniskAnlaeg,
            Grund = grund,
            GrundJordstykker = jordstykker,
            BygningEjendomsrelation = bygningEjendomsrelation,
            Ejendomsrelation = ejendomsrelation
        };
    }

    internal sealed class LookupMethodCalls
    {
        public required string Overview { get; init; }

        public required string AdresseopslagDar { get; init; }

        public required string Adresseopslag { get; init; }

        public required string KvHxInput { get; init; }

        public required string Husnummer { get; init; }

        public required string Bygning { get; init; }

        public required string Enhed { get; init; }

        public required string Etage { get; init; }

        public required string Opgang { get; init; }

        public required string TekniskAnlaeg { get; init; }

        public required string Grund { get; init; }

        public required string GrundJordstykker { get; init; }

        public required string BygningEjendomsrelation { get; init; }

        public required string Ejendomsrelation { get; init; }
    }
}
