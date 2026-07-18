namespace TheNerdCollective.MudComponents.ResponsiveTypography;

public interface INerdTypographyPackStore
{
    Task SaveAsync(NerdTypographyPack pack, CancellationToken cancellationToken = default);
    Task<NerdTypographyPack?> LoadAsync(string clientId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> ListAsync(CancellationToken cancellationToken = default);
}
