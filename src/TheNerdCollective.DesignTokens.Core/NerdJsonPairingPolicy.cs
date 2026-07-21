namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Pairing policy hydrated from <see cref="NerdTokenPack.ApprovedPairings"/>.
/// </summary>
public sealed class NerdJsonPairingPolicy : INerdPairingPolicy
{
    public NerdJsonPairingPolicy(string brandGuideName, IReadOnlyList<NerdApprovedPairing> approvedPairings)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(brandGuideName);
        ArgumentNullException.ThrowIfNull(approvedPairings);

        BrandGuideName = brandGuideName;
        _approvedPairings = approvedPairings;
        _approvedLookup = approvedPairings
            .Select(pair => $"{pair.Content}|{pair.Surface}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        _contentBySurface = approvedPairings
            .GroupBy(pair => pair.Surface, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.Select(pair => pair.Content).Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
                StringComparer.OrdinalIgnoreCase);
    }

    private readonly IReadOnlyList<NerdApprovedPairing> _approvedPairings;
    private readonly HashSet<string> _approvedLookup;
    private readonly Dictionary<string, List<string>> _contentBySurface;

    public string BrandGuideName { get; }

    public static bool TryCreate(NerdTokenPack pack, out NerdJsonPairingPolicy? policy)
    {
        ArgumentNullException.ThrowIfNull(pack);
        if (pack.ApprovedPairings.Count == 0 || string.IsNullOrWhiteSpace(pack.PairingGuideName))
        {
            policy = null;
            return false;
        }

        policy = new NerdJsonPairingPolicy(pack.PairingGuideName, pack.ApprovedPairings);
        return true;
    }

    public static NerdJsonPairingPolicy FromPolicy(INerdPairingPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);
        var pairings = policy.GetApprovedPairings()
            .Select(pair => new NerdApprovedPairing(pair.Content, pair.Surface))
            .ToList();
        return new NerdJsonPairingPolicy(policy.BrandGuideName, pairings);
    }

    public bool IsActive(NerdDesignTokenOptions options) => options.Colors.Count > 0;

    public string SuggestContentToken(string surfaceToken, NerdDesignTokenOptions options)
    {
        if (_contentBySurface.TryGetValue(surfaceToken, out var contents) && contents.Count > 0)
        {
            return contents[0];
        }

        return NerdTokenPairingTools.SuggestContentToken(surfaceToken, options);
    }

    public bool IsBrandApprovedPairing(string contentToken, string surfaceToken) =>
        _approvedLookup.Contains($"{contentToken}|{surfaceToken}");

    public IReadOnlyList<string> GetApprovedContentTokens(string surfaceToken, NerdDesignTokenOptions options)
    {
        if (_contentBySurface.TryGetValue(surfaceToken, out var contents) && contents.Count > 0)
        {
            return contents
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        return [SuggestContentToken(surfaceToken, options)];
    }

    public IReadOnlyList<(string Content, string Surface)> GetApprovedPairings() =>
        _approvedPairings
            .Select(pair => (pair.Content, pair.Surface))
            .ToList();

    public string SuggestActionToken(
        NerdDesignTokenOptions options,
        string surfaceToken,
        string contentToken) =>
        NerdTokenPairingTools.SuggestActionToken(options, surfaceToken, contentToken);

    public string ResolveForegroundColor(string tokenName, NerdDesignTokenOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenName);
        ArgumentNullException.ThrowIfNull(options);
        var token = options.Colors[tokenName];
        var paint = token.Light ?? token.Value;

        // Content means "text on this token when used as a surface".
        // Use it as pairing foreground only when the token is not its own surface paint
        // (e.g. kridt surface tint vs kridt text ink; skov keeps its green value).
        if (!string.IsNullOrWhiteSpace(token.Content) &&
            !string.Equals(token.Content, paint, StringComparison.OrdinalIgnoreCase) &&
            string.IsNullOrWhiteSpace(token.Surface))
        {
            return token.Content;
        }

        return paint;
    }

    public string ResolveSurfaceColor(string tokenName, NerdDesignTokenOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenName);
        ArgumentNullException.ThrowIfNull(options);
        var token = options.Colors[tokenName];
        return token.Surface ?? token.Light ?? token.Value;
    }
}
