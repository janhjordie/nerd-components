using TheNerdCollective.Integrations.Dar.Models;

namespace TheNerdCollective.Integrations.Dar.TestWeb.Models;

public sealed class DarFullLookupResult
{
    public required TimeSpan Duration { get; init; }

    public AdresseopslagResult? Adresseopslag { get; init; }

    public HusnummerLookupResult? Husnummer { get; init; }

    public BygningDto? Bygning { get; init; }

    public IReadOnlyList<EnhedDto> Enheder { get; init; } = [];

    public IReadOnlyList<EtageDto> Etager { get; init; } = [];

    public IReadOnlyList<OpgangDto> Opgange { get; init; } = [];

    public IReadOnlyList<TekniskAnlaegDto> TekniskeAnlaeg { get; init; } = [];

    public GrundDto? Grund { get; init; }

    public IReadOnlyList<GrundJordstykkeDto> GrundJordstykker { get; init; } = [];

    public IReadOnlyList<BygningEjendomsrelationDto> BygningEjendomsrelationer { get; init; } = [];

    public IReadOnlyList<EjendomsrelationDto> Ejendomsrelationer { get; init; } = [];
}
