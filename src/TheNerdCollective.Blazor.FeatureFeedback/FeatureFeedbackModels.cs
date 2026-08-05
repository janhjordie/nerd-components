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
    Declined,
    /// <summary>Soft-deleted — hidden from the public board, retained for admin restore/hard-delete.</summary>
    Deleted
}

public sealed record FeatureIdeaDto(
    Guid Id,
    string Title,
    string Description,
    int VoteCount,
    bool VotedByCurrentUser,
    string AuthorDisplayName,
    DateTimeOffset CreatedAtUtc,
    FeatureIdeaStatus Status,
    DateOnly? PlannedReleaseDate = null);

public sealed record CreateFeatureIdeaRequest(
    string Title,
    string Description,
    string AuthorDisplayName);

public sealed record UpdateFeatureIdeaRequest(
    FeatureIdeaStatus Status,
    DateOnly? PlannedReleaseDate);

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
        bool includeDeleted = false,
        CancellationToken cancellationToken = default);

    Task<FeatureIdeaMutationResult> CreateAsync(
        CreateFeatureIdeaRequest request,
        string userId,
        CancellationToken cancellationToken = default);

    Task<FeatureIdeaMutationResult> ToggleVoteAsync(
        Guid ideaId,
        string userId,
        CancellationToken cancellationToken = default);

    Task<FeatureIdeaMutationResult> UpdateAsync(
        Guid ideaId,
        UpdateFeatureIdeaRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Marks the idea as <see cref="FeatureIdeaStatus.Deleted"/> (hidden from public lists).</summary>
    Task<FeatureIdeaMutationResult> SoftDeleteAsync(
        Guid ideaId,
        CancellationToken cancellationToken = default);

    /// <summary>Permanently removes the idea and its votes.</summary>
    Task<FeatureIdeaMutationResult> HardDeleteAsync(
        Guid ideaId,
        CancellationToken cancellationToken = default);

    /// <summary>Restores a soft-deleted idea to <see cref="FeatureIdeaStatus.Open"/>.</summary>
    Task<FeatureIdeaMutationResult> RestoreAsync(
        Guid ideaId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Host implements this to decide who may open the Feature Feedback admin UI.
/// </summary>
public interface IFeatureFeedbackAdminAccess
{
    Task<bool> CanAdministerAsync(
        System.Security.Claims.ClaimsPrincipal user,
        CancellationToken cancellationToken = default);
}
