namespace TheNerdCollective.Brand.Tnc;

/// <summary>
/// Approved foreground/background pairings derived from TNC layout recipes.
/// </summary>
public sealed class NerdTncPairingPolicy : INerdPairingPolicy
{
    public const string NavyToken = "navy";
    public const string CoralToken = "coral";
    public const string SnowToken = "snow";
    public const string InkToken = "ink";
    public const string ChalkToken = "chalk";

    public string BrandGuideName => "TNC";

    private static readonly HashSet<string> DarkSurfaces = new(StringComparer.OrdinalIgnoreCase)
    {
        NavyToken, CoralToken, InkToken
    };

    private static readonly HashSet<string> ApprovedPairings = new(StringComparer.OrdinalIgnoreCase)
    {
        $"{ChalkToken}|{NavyToken}",
        $"{InkToken}|{SnowToken}",
        $"{CoralToken}|{NavyToken}",
        $"{ChalkToken}|{CoralToken}"
    };

    public bool IsActive(NerdDesignTokenOptions options) =>
        options.Colors.ContainsKey(NavyToken) && options.Colors.ContainsKey(CoralToken);

    public string SuggestContentToken(string surfaceToken, NerdDesignTokenOptions options) =>
        string.Equals(surfaceToken, SnowToken, StringComparison.OrdinalIgnoreCase)
            ? InkToken
            : ChalkToken;

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
        (ChalkToken, NavyToken),
        (InkToken, SnowToken),
        (CoralToken, NavyToken),
        (ChalkToken, CoralToken)
    ];

    public string SuggestActionToken(NerdDesignTokenOptions options, string surfaceToken, string contentToken)
    {
        if (options.Colors.ContainsKey(CoralToken) &&
            !string.Equals(CoralToken, surfaceToken, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(CoralToken, contentToken, StringComparison.OrdinalIgnoreCase))
        {
            return CoralToken;
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

        if (string.Equals(tokenName, ChalkToken, StringComparison.OrdinalIgnoreCase))
        {
            return NerdTncDesignTokenPresets.Snow;
        }

        var token = options.Colors[tokenName];
        return token.Content ?? token.Value;
    }

    public string ResolveSurfaceColor(string tokenName, NerdDesignTokenOptions options) =>
        options.Colors[tokenName].Value;
}
