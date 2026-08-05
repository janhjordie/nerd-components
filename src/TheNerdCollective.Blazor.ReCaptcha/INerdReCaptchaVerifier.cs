namespace TheNerdCollective.Blazor.ReCaptcha;

public sealed record ReCaptchaVerifyResult(bool Success, string? ErrorCode = null);

public interface INerdReCaptchaVerifier
{
    NerdReCaptchaMode Mode { get; }

    /// <summary>Public site key when using Google; null for simple mode.</summary>
    string? SiteKey { get; }

    Task<SimpleChallenge> CreateSimpleChallengeAsync(CancellationToken cancellationToken = default);

    Task<ReCaptchaVerifyResult> VerifyAsync(string? responseToken, CancellationToken cancellationToken = default);
}

public sealed record SimpleChallenge(string ChallengeId, int Left, int Right, string Prompt);
