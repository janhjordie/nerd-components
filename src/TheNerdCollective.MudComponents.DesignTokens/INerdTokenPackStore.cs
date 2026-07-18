namespace TheNerdCollective.MudComponents.DesignTokens;

public interface INerdTokenPackStore
{
    Task SaveAsync(NerdTokenPack pack, CancellationToken cancellationToken = default);
    Task<NerdTokenPack?> LoadAsync(string clientId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> ListAsync(CancellationToken cancellationToken = default);
}
