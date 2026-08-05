namespace TheNerdCollective.Blazor.FeatureFeedback;

public enum FeatureIdeaSort
{
    MostVoted,
    Newest
}

public enum FeatureIdeaStatus
{
    Open,
    Planned,
    InProgress,
    Done,
    Declined
}

public sealed record FeatureIdeaDto(
    Guid Id,
    string Title,
    string Description,
    int VoteCount,
    bool VotedByCurrentUser,
    string AuthorDisplayName,
    DateTimeOffset CreatedAtUtc,
    FeatureIdeaStatus Status);

public sealed record CreateFeatureIdeaRequest(
    string Title,
    string Description,
    string AuthorDisplayName);

public sealed record FeatureIdeaMutationResult(
    bool Success,
    FeatureIdeaDto? Idea = null,
    string? ErrorCode = null);

public interface IFeatureFeedbackStore
{
    Task<IReadOnlyList<FeatureIdeaDto>> ListAsync(
        FeatureIdeaSort sort = FeatureIdeaSort.MostVoted,
        string? search = null,
        string? currentUserId = null,
        CancellationToken cancellationToken = default);

    Task<FeatureIdeaMutationResult> CreateAsync(
        CreateFeatureIdeaRequest request,
        string userId,
        CancellationToken cancellationToken = default);

    Task<FeatureIdeaMutationResult> ToggleVoteAsync(
        Guid ideaId,
        string userId,
        CancellationToken cancellationToken = default);
}
