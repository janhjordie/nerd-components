namespace TheNerdCollective.Brand.Acme;

/// <summary>
/// Approved foreground/background pairings for the Acme sample brand.
/// </summary>
public sealed class NerdAcmePairingPolicy : INerdPairingPolicy
{
    public const string ForestToken = "forest";
    public const string SunriseToken = "sunrise";
    public const string CloudToken = "cloud";
    public const string InkToken = "ink";

    public string BrandGuideName => "Acme";

    private static readonly HashSet<string> DarkSurfaces = new(StringComparer.OrdinalIgnoreCase)
    {
        ForestToken, InkToken
    };

    private static readonly HashSet<string> ApprovedPairings = new(StringComparer.OrdinalIgnoreCase)
    {
        $"{InkToken}|{CloudToken}",
        $"{CloudToken}|{InkToken}",
        $"{CloudToken}|{ForestToken}"
    };

    public bool IsActive(NerdDesignTokenOptions options) =>
        options.Colors.ContainsKey(ForestToken) && options.Colors.ContainsKey(CloudToken);

    public string SuggestContentToken(string surfaceToken, NerdDesignTokenOptions options) =>
        DarkSurfaces.Contains(surfaceToken) ? CloudToken : InkToken;

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
        (InkToken, CloudToken),
        (CloudToken, InkToken),
        (CloudToken, ForestToken)
    ];

    public string SuggestActionToken(NerdDesignTokenOptions options, string surfaceToken, string contentToken)
    {
        if (options.Colors.ContainsKey(ForestToken) &&
            !string.Equals(ForestToken, surfaceToken, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(ForestToken, contentToken, StringComparison.OrdinalIgnoreCase))
        {
            return ForestToken;
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
