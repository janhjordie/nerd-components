namespace TheNerdCollective.Brand.Dnf;

/// <summary>
/// Approved foreground/background pairings from the DNF 2025 identity guide.
/// </summary>
public sealed class NerdDnfPairingPolicy : INerdPairingPolicy
{
    public const string KridtToken = "kridt";
    public const string SkovToken = "skov";

    public string BrandGuideName => "DNF";

    private static readonly HashSet<string> DarkSurfaces = new(StringComparer.OrdinalIgnoreCase)
    {
        "jord", "ler", "hav", "skov", "blad"
    };

    private static readonly HashSet<string> LightSurfaces = new(StringComparer.OrdinalIgnoreCase)
    {
        "kridt", "kridt-lys", "sol", "morgenrode", "himmel", "flod", "graes"
    };

    private static readonly HashSet<string> ApprovedPairings = new(StringComparer.OrdinalIgnoreCase)
    {
        $"{KridtToken}|jord",
        $"{KridtToken}|ler",
        $"{KridtToken}|hav",
        $"{KridtToken}|{SkovToken}",
        $"{KridtToken}|blad",
        $"{SkovToken}|kridt",
        $"{SkovToken}|kridt-lys",
        $"{SkovToken}|sol",
        $"{SkovToken}|morgenrode",
        $"{SkovToken}|himmel",
        $"{SkovToken}|flod",
        $"{SkovToken}|graes"
    };

    public bool IsActive(NerdDesignTokenOptions options) =>
        options.Colors.ContainsKey(SkovToken) && options.Colors.ContainsKey(KridtToken);

    public string SuggestContentToken(string surfaceToken, NerdDesignTokenOptions options) =>
        DarkSurfaces.Contains(surfaceToken) ? KridtToken : SkovToken;

    public bool IsBrandApprovedPairing(string contentToken, string surfaceToken) =>
        ApprovedPairings.Contains($"{contentToken}|{surfaceToken}");

    public IReadOnlyList<string> GetApprovedContentTokens(string surfaceToken, NerdDesignTokenOptions options)
    {
        var suggested = SuggestContentToken(surfaceToken, options);
        return ApprovedPairings
            .Select(pair => pair.Split('|'))
            .Where(parts => parts.Length == 2 &&
                            string.Equals(parts[1], surfaceToken, StringComparison.OrdinalIgnoreCase))
            .Select(parts => parts[0])
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .DefaultIfEmpty(suggested)
            .ToList();
    }

    public IReadOnlyList<(string Content, string Surface)> GetApprovedPairings() =>
    [
        (KridtToken, "jord"),
        (KridtToken, "ler"),
        (KridtToken, "hav"),
        (KridtToken, SkovToken),
        (KridtToken, "blad"),
        (SkovToken, "kridt"),
        (SkovToken, "kridt-lys"),
        (SkovToken, "sol"),
        (SkovToken, "morgenrode"),
        (SkovToken, "himmel"),
        (SkovToken, "flod"),
        (SkovToken, "graes")
    ];

    public string SuggestActionToken(NerdDesignTokenOptions options, string surfaceToken, string contentToken)
    {
        if (options.Colors.ContainsKey("himmel") &&
            !string.Equals("himmel", surfaceToken, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals("himmel", contentToken, StringComparison.OrdinalIgnoreCase))
        {
            return "himmel";
        }

        return options.Colors.Keys
            .FirstOrDefault(name =>
                !string.Equals(name, surfaceToken, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(name, contentToken, StringComparison.OrdinalIgnoreCase))
            ?? contentToken;
    }

    public string ResolveForegroundColor(string tokenName, NerdDesignTokenOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenName);
        ArgumentNullException.ThrowIfNull(options);

        if (string.Equals(tokenName, KridtToken, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(tokenName, "kridt-lys", StringComparison.OrdinalIgnoreCase))
        {
            return NerdDnfDesignTokenPresets.KridtText;
        }

        if (string.Equals(tokenName, SkovToken, StringComparison.OrdinalIgnoreCase))
        {
            return NerdDnfDesignTokenPresets.SkovText;
        }

        return options.Colors[tokenName].Value;
    }

    public string ResolveSurfaceColor(string tokenName, NerdDesignTokenOptions options) =>
        options.Colors[tokenName].Value;
}
