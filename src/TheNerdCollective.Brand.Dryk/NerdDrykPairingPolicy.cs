namespace TheNerdCollective.Brand.Dryk;

/// <summary>
/// Approved foreground/background pairings from the DRYK web identity.
/// </summary>
public sealed class NerdDrykPairingPolicy : INerdPairingPolicy
{
    public const string KridtToken = "kridt";
    public const string SkovToken = "skov";
    public const string InkToken = "ink";

    public string BrandGuideName => "DRYK";

    private static readonly HashSet<string> DarkSurfaces = new(StringComparer.OrdinalIgnoreCase)
    {
        SkovToken
    };

    private static readonly HashSet<string> LightSurfaces = new(StringComparer.OrdinalIgnoreCase)
    {
        "mint", "graes", "sten", KridtToken, InkToken, "aerter", "havre", "sol"
    };

    private static readonly HashSet<string> ApprovedPairings = new(StringComparer.OrdinalIgnoreCase)
    {
        $"{KridtToken}|{SkovToken}",
        $"{InkToken}|{KridtToken}",
        $"{InkToken}|mint",
        $"{InkToken}|sten",
        $"{InkToken}|graes",
        $"{SkovToken}|{KridtToken}",
        $"{SkovToken}|mint",
        $"{SkovToken}|sten",
        $"{SkovToken}|graes",
        $"{SkovToken}|havre",
        $"{KridtToken}|sol"
    };

    public bool IsActive(NerdDesignTokenOptions options) =>
        options.Colors.ContainsKey(SkovToken) && options.Colors.ContainsKey(KridtToken);

    public string SuggestContentToken(string surfaceToken, NerdDesignTokenOptions options) =>
        DarkSurfaces.Contains(surfaceToken) ? KridtToken : InkToken;

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
        (KridtToken, SkovToken),
        (InkToken, KridtToken),
        (InkToken, "mint"),
        (InkToken, "sten"),
        (InkToken, "graes"),
        (SkovToken, KridtToken),
        (SkovToken, "mint"),
        (SkovToken, "sten"),
        (SkovToken, "graes"),
        (SkovToken, "havre"),
        (KridtToken, "sol")
    ];

    public string SuggestActionToken(NerdDesignTokenOptions options, string surfaceToken, string contentToken)
    {
        if (options.Colors.ContainsKey("graes") &&
            !string.Equals("graes", surfaceToken, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals("graes", contentToken, StringComparison.OrdinalIgnoreCase))
        {
            return "graes";
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

        if (string.Equals(tokenName, KridtToken, StringComparison.OrdinalIgnoreCase))
        {
            return NerdDrykDesignTokenPresets.KridtText;
        }

        if (string.Equals(tokenName, InkToken, StringComparison.OrdinalIgnoreCase))
        {
            return NerdDrykDesignTokenPresets.InkText;
        }

        if (string.Equals(tokenName, SkovToken, StringComparison.OrdinalIgnoreCase))
        {
            return NerdDrykDesignTokenPresets.SkovText;
        }

        return options.Colors[tokenName].Value;
    }

    public string ResolveSurfaceColor(string tokenName, NerdDesignTokenOptions options) =>
        options.Colors[tokenName].Value;
}
