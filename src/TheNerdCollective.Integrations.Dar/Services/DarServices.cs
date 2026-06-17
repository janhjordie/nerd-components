using TheNerdCollective.Integrations.Dar.Services.Bbr;
using TheNerdCollective.Integrations.Dar.Services.Dar;

namespace TheNerdCollective.Integrations.Dar.Services
{
    /// <summary>Samlet adgang til DAR- og BBR-services.</summary>
    public sealed class DarServices
    {
        public DarServices(DarRegister dar, BbrServices bbr)
        {
            Dar = dar;
            Bbr = bbr;
        }

        public DarRegister Dar { get; }

        public BbrServices Bbr { get; }
    }

    /// <summary>DAR-services (adresser).</summary>
    public sealed class DarRegister
    {
        public DarRegister(
            DarAdresseopslagService adresseopslag,
            DarHusnummerService husnummer,
            DarAddressAutocompleteService autocomplete,
            DarKommuneService kommune)
        {
            Adresseopslag = adresseopslag;
            Husnummer = husnummer;
            Autocomplete = autocomplete;
            Kommune = kommune;
        }

        public DarAdresseopslagService Adresseopslag { get; }

        public DarHusnummerService Husnummer { get; }

        public DarAddressAutocompleteService Autocomplete { get; }

        public DarKommuneService Kommune { get; }
    }

    /// <summary>BBR-services (bygning, enheder, etager m.m.).</summary>
    public sealed class BbrServices
    {
        public BbrServices(
            BbrBygningService bygning,
            BbrEnhedService enhed,
            BbrEtageService etage,
            BbrOpgangService opgang,
            BbrTekniskAnlaegService tekniskAnlaeg,
            BbrGrundService grund,
            BbrEjendomsrelationService ejendomsrelation)
        {
            Bygning = bygning;
            Enhed = enhed;
            Etage = etage;
            Opgang = opgang;
            TekniskAnlaeg = tekniskAnlaeg;
            Grund = grund;
            Ejendomsrelation = ejendomsrelation;
        }

        public BbrBygningService Bygning { get; }

        public BbrEnhedService Enhed { get; }

        public BbrEtageService Etage { get; }

        public BbrOpgangService Opgang { get; }

        public BbrTekniskAnlaegService TekniskAnlaeg { get; }

        public BbrGrundService Grund { get; }

        public BbrEjendomsrelationService Ejendomsrelation { get; }
    }
}
