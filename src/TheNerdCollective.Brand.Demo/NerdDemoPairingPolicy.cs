namespace TheNerdCollective.Brand.Demo;

/// <summary>
/// Approved foreground/background pairings for the Demo sample brand.
/// </summary>
public sealed class NerdDemoPairingPolicy : INerdPairingPolicy
{
    public const string VioletToken = "violet";
    public const string SkyToken = "sky";
    public const string PaperToken = "paper";
    public const string SlateToken = "slate";

    public string BrandGuideName => "Demo";

    private static readonly HashSet<string> DarkSurfaces = new(StringComparer.OrdinalIgnoreCase)
    {
        VioletToken, SlateToken
    };

    private static readonly HashSet<string> ApprovedPairings = new(StringComparer.OrdinalIgnoreCase)
    {
        $"{PaperToken}|{SlateToken}",
        $"{PaperToken}|{VioletToken}",
        $"{SlateToken}|{PaperToken}",
        $"{SkyToken}|{SlateToken}"
    };

    public bool IsActive(NerdDesignTokenOptions options) =>
        options.Colors.ContainsKey(VioletToken) && options.Colors.ContainsKey(PaperToken);

    public string SuggestContentToken(string surfaceToken, NerdDesignTokenOptions options) =>
        DarkSurfaces.Contains(surfaceToken) ? PaperToken : SlateToken;

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
        (PaperToken, SlateToken),
        (PaperToken, VioletToken),
        (SlateToken, PaperToken),
        (SkyToken, SlateToken)
    ];

    public string SuggestActionToken(NerdDesignTokenOptions options, string surfaceToken, string contentToken)
    {
        if (options.Colors.ContainsKey(SkyToken) &&
            !string.Equals(SkyToken, surfaceToken, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(SkyToken, contentToken, StringComparison.OrdinalIgnoreCase))
        {
            return SkyToken;
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

        var token = options.Colors[tokenName];
        return token.Content ?? token.Value;
    }

    public string ResolveSurfaceColor(string tokenName, NerdDesignTokenOptions options) =>
        options.Colors[tokenName].Value;
}
