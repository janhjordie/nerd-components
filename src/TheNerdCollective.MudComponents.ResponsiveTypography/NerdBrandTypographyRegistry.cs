namespace TheNerdCollective.MudComponents.ResponsiveTypography;

/// <summary>
/// Registry of installed <see cref="INerdBrandTypographyPack"/> instances for catalog brand switching.
/// </summary>
public sealed class NerdBrandTypographyRegistry
{
    public static NerdBrandTypographyRegistry Instance { get; } = new();

    private readonly Dictionary<string, INerdBrandTypographyPack> _packs = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<INerdBrandTypographyPack> Packs => _packs.Values;

    public void Register(INerdBrandTypographyPack pack)
    {
        ArgumentNullException.ThrowIfNull(pack);
        ArgumentException.ThrowIfNullOrWhiteSpace(pack.Id);
        _packs[pack.Id] = pack;
    }

    public void Reset()
    {
        _packs.Clear();
    }

    public bool TryGet(string id, out INerdBrandTypographyPack? pack) =>
        _packs.TryGetValue(id, out pack);

    public INerdBrandTypographyPack GetRequired(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        if (!_packs.TryGetValue(id, out var pack))
        {
            throw new ArgumentException(
                $"Unknown brand typography pack '{id}'. Register it with NerdBrandTypographyRegistry.Instance.Register(...) " +
                "or call RegisterBrandTypography / AddNerdBrandTypographyPacks from the brand package.",
                nameof(id));
        }

        return pack;
    }

    public void Configure(string id, NerdResponsiveTypographyOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var pack = GetRequired(id);
        options.Typography.Reset();
        pack.Configure(options.Typography);
    }
}
