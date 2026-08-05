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
        var state = new IdeaState
        {
            Id = id,
            Title = title,
            Description = description,
            AuthorDisplayName = string.IsNullOrWhiteSpace(request.AuthorDisplayName)
                ? "Customer"
                : request.AuthorDisplayName.Trim(),
            AuthorUserId = userId,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            Status = FeatureIdeaStatus.Open
        };

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

    public Task<FeatureIdeaMutationResult> UpdateAsync(
        Guid ideaId,
        UpdateFeatureIdeaRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_ideas.TryGetValue(ideaId, out var state))
        {
            return Task.FromResult(new FeatureIdeaMutationResult(false, ErrorCode: "not-found"));
        }

        state.Status = request.Status;
        state.PlannedReleaseDate = request.PlannedReleaseDate;
        return Task.FromResult(new FeatureIdeaMutationResult(true, ToDto(state, null)));
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
            state.Status,
            state.PlannedReleaseDate);

    private sealed class IdeaState
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = "";
        public string Description { get; init; } = "";
        public string AuthorDisplayName { get; init; } = "";
        public string AuthorUserId { get; init; } = "";
        public DateTimeOffset CreatedAtUtc { get; init; }
        public FeatureIdeaStatus Status { get; set; }
        public DateOnly? PlannedReleaseDate { get; set; }
        public ConcurrentDictionary<string, byte> Votes { get; } = new();
    }
}
