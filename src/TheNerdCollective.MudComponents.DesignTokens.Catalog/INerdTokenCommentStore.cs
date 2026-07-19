namespace TheNerdCollective.MudComponents.DesignTokens;

public interface INerdTokenCommentStore
{
    Task<IReadOnlyDictionary<string, string>> LoadAsync(string clientId, CancellationToken cancellationToken = default);

    Task SaveAsync(string clientId, IReadOnlyDictionary<string, string> comments, CancellationToken cancellationToken = default);
}
