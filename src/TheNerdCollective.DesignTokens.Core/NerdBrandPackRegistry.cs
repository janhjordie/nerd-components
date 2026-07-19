namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Registry of installed <see cref="INerdBrandPack"/> instances for preset loading and catalog brand switching.
/// </summary>
public sealed class NerdBrandPackRegistry
{
    public static NerdBrandPackRegistry Instance { get; } = new();

    private readonly Dictionary<string, INerdBrandPack> _packs = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<INerdBrandPack> Packs => _packs.Values;

    public void Register(INerdBrandPack pack)
    {
        ArgumentNullException.ThrowIfNull(pack);
        ArgumentException.ThrowIfNullOrWhiteSpace(pack.Id);
        _packs[pack.Id] = pack;
    }

    public void Reset()
    {
        _packs.Clear();
    }

    public INerdBrandPack GetRequired(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        if (!_packs.TryGetValue(id, out var pack))
        {
            throw new ArgumentException(
                $"Unknown brand pack '{id}'. Register it with NerdBrandPackRegistry.Instance.Register(...) " +
                "or call AddNerdDesignTokenBrandPacks / AddNerdDnfBrand from the brand package.",
                nameof(id));
        }

        return pack;
    }

    public bool TryGet(string id, out INerdBrandPack? pack) =>
        _packs.TryGetValue(id, out pack);

    public void Configure(string id, NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var pack = GetRequired(id);
        var brand = CreateBrandOptions(pack);
        brand.CopyHostSettingsFrom(options);
        options.ReplaceWith(brand);
    }

    private static NerdDesignTokenOptions CreateBrandOptions(INerdBrandPack pack)
    {
        var options = new NerdDesignTokenOptions();
        pack.Configure(options);
        options.PairingPolicy = pack.PairingPolicy;
        options.ActiveBrandPackId = pack.Id;
        options.ActiveBrandIdentityVersion = pack.IdentityVersion;
        return options;
    }

    public NerdTokenPack CreateTokenPack(string id, string clientId = "default")
    {
        var pack = GetRequired(id);
        return NerdTokenPack.FromOptions(CreateBrandOptions(pack), clientId);
    }
}
