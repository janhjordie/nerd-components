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
            Address = new DarAddressResolutionService(dar.Autocomplete, dar.Adresseopslag);
        }

        public DarRegister Dar { get; }

        public BbrServices Bbr { get; }

        /// <summary>Autocomplete → bedste match → ids, KvHxInput og BBR-nøgler.</summary>
        public DarAddressResolutionService Address { get; }
    }

    /// <summary>DAR-services (adresser).</summary>
    public sealed class DarRegister
    {
        public DarRegister(
            DarAdresseopslagService adresseopslag,
            DarHusnummerService husnummer,
            DarAddressAutocompleteService autocomplete,
            DarKommuneService kommune,
            DarRegionService region,
            DarPostnummerService postnummer)
        {
            Adresseopslag = adresseopslag;
            Husnummer = husnummer;
            Autocomplete = autocomplete;
            Kommune = kommune;
            Region = region;
            Postnummer = postnummer;
        }

        public DarAdresseopslagService Adresseopslag { get; }

        public DarHusnummerService Husnummer { get; }

        public DarAddressAutocompleteService Autocomplete { get; }

        public DarKommuneService Kommune { get; }

        public DarRegionService Region { get; }

        public DarPostnummerService Postnummer { get; }
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
