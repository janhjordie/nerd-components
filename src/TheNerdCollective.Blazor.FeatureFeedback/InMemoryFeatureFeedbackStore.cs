using System.Collections.Concurrent;

namespace TheNerdCollective.Blazor.FeatureFeedback;

/// <summary>
/// In-memory store for demos and tests. Hosts should replace with a persistent implementation.
/// </summary>
public sealed class InMemoryFeatureFeedbackStore : IFeatureFeedbackStore
{
    private readonly ConcurrentDictionary<Guid, IdeaState> _ideas = new();

    public Task<IReadOnlyList<FeatureIdeaDto>> ListAsync(
        FeatureIdeaSort sort = FeatureIdeaSort.MostVoted,
        string? search = null,
        string? currentUserId = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IEnumerable<IdeaState> query = _ideas.Values;
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(idea =>
                idea.Title.Contains(search, StringComparison.OrdinalIgnoreCase)
                || idea.Description.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        query = sort == FeatureIdeaSort.Newest
            ? query.OrderByDescending(idea => idea.CreatedAtUtc)
            : query.OrderByDescending(idea => idea.Votes.Count).ThenByDescending(idea => idea.CreatedAtUtc);

        var list = query
            .Select(idea => ToDto(idea, currentUserId))
            .ToList();

        return Task.FromResult<IReadOnlyList<FeatureIdeaDto>>(list);
    }

    public Task<FeatureIdeaMutationResult> CreateAsync(
        CreateFeatureIdeaRequest request,
        string userId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Task.FromResult(new FeatureIdeaMutationResult(false, ErrorCode: "unauthenticated"));
        }

        var title = request.Title?.Trim() ?? "";
        var description = request.Description?.Trim() ?? "";
        if (title.Length is < 3 or > 120)
        {
            return Task.FromResult(new FeatureIdeaMutationResult(false, ErrorCode: "invalid-title"));
        }

        if (description.Length is < 10 or > 2000)
        {
            return Task.FromResult(new FeatureIdeaMutationResult(false, ErrorCode: "invalid-description"));
        }

        var id = Guid.NewGuid();
        var state = new IdeaState(
            id,
            title,
            description,
            string.IsNullOrWhiteSpace(request.AuthorDisplayName) ? "Customer" : request.AuthorDisplayName.Trim(),
            userId,
            DateTimeOffset.UtcNow,
            FeatureIdeaStatus.Open,
            new ConcurrentDictionary<string, byte>());

        state.Votes[userId] = 0;
        _ideas[id] = state;

        return Task.FromResult(new FeatureIdeaMutationResult(true, ToDto(state, userId)));
    }

    public Task<FeatureIdeaMutationResult> ToggleVoteAsync(
        Guid ideaId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Task.FromResult(new FeatureIdeaMutationResult(false, ErrorCode: "unauthenticated"));
        }

        if (!_ideas.TryGetValue(ideaId, out var state))
        {
            return Task.FromResult(new FeatureIdeaMutationResult(false, ErrorCode: "not-found"));
        }

        if (!state.Votes.TryRemove(userId, out _))
        {
            state.Votes[userId] = 0;
        }

        return Task.FromResult(new FeatureIdeaMutationResult(true, ToDto(state, userId)));
    }

    private static FeatureIdeaDto ToDto(IdeaState state, string? currentUserId) =>
        new(
            state.Id,
            state.Title,
            state.Description,
            state.Votes.Count,
            currentUserId is not null && state.Votes.ContainsKey(currentUserId),
            state.AuthorDisplayName,
            state.CreatedAtUtc,
            state.Status);

    private sealed record IdeaState(
        Guid Id,
        string Title,
        string Description,
        string AuthorDisplayName,
        string AuthorUserId,
        DateTimeOffset CreatedAtUtc,
        FeatureIdeaStatus Status,
        ConcurrentDictionary<string, byte> Votes);
}
