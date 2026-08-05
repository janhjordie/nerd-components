using System.Collections.Concurrent;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace TheNerdCollective.Blazor.ReCaptcha;

public sealed class NerdReCaptchaVerifier : INerdReCaptchaVerifier
{
    private const string SimplePrefix = "simple:";
    private readonly NerdReCaptchaOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ConcurrentDictionary<string, SimpleChallengeState> _challenges = new();

    public NerdReCaptchaVerifier(IOptions<NerdReCaptchaOptions> options, IHttpClientFactory httpClientFactory)
    {
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
    }

    public NerdReCaptchaMode Mode => _options.ResolvedMode;

    public string? SiteKey => Mode == NerdReCaptchaMode.GoogleV2 ? _options.SiteKey : null;

    public Task<SimpleChallenge> CreateSimpleChallengeAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CleanupExpired();

        var left = Random.Shared.Next(1, 10);
        var right = Random.Shared.Next(1, 10);
        var id = Guid.NewGuid().ToString("N");
        var expires = DateTimeOffset.UtcNow.Add(_options.SimpleChallengeLifetime);
        _challenges[id] = new SimpleChallengeState(left + right, expires);

        var challenge = new SimpleChallenge(
            id,
            left,
            right,
            $"What is {left} + {right}?");

        return Task.FromResult(challenge);
    }

    public async Task<ReCaptchaVerifyResult> VerifyAsync(
        string? responseToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(responseToken))
        {
            return new ReCaptchaVerifyResult(false, "missing-input-response");
        }

        if (Mode == NerdReCaptchaMode.Simple || responseToken.StartsWith(SimplePrefix, StringComparison.Ordinal))
        {
            return VerifySimple(responseToken);
        }

        return await VerifyGoogleAsync(responseToken, cancellationToken).ConfigureAwait(false);
    }

    private ReCaptchaVerifyResult VerifySimple(string responseToken)
    {
        // Format: simple:{challengeId}:{answer}
        if (!responseToken.StartsWith(SimplePrefix, StringComparison.Ordinal))
        {
            return new ReCaptchaVerifyResult(false, "invalid-simple-token");
        }

        var parts = responseToken.Split(':', 3, StringSplitOptions.None);
        if (parts.Length != 3
            || !int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var answer))
        {
            return new ReCaptchaVerifyResult(false, "invalid-simple-token");
        }

        var challengeId = parts[1];
        if (!_challenges.TryRemove(challengeId, out var state))
        {
            return new ReCaptchaVerifyResult(false, "challenge-not-found");
        }

        if (state.ExpiresAtUtc < DateTimeOffset.UtcNow)
        {
            return new ReCaptchaVerifyResult(false, "challenge-expired");
        }

        return state.ExpectedAnswer == answer
            ? new ReCaptchaVerifyResult(true)
            : new ReCaptchaVerifyResult(false, "incorrect-answer");
    }

    private async Task<ReCaptchaVerifyResult> VerifyGoogleAsync(
        string responseToken,
        CancellationToken cancellationToken)
    {
        if (!_options.HasGoogleKeys)
        {
            return new ReCaptchaVerifyResult(false, "missing-google-keys");
        }

        var client = _httpClientFactory.CreateClient(nameof(NerdReCaptchaVerifier));
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["secret"] = _options.SecretKey!,
            ["response"] = responseToken
        });

        using var response = await client
            .PostAsync(_options.VerifyUrl, content, cancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            return new ReCaptchaVerifyResult(false, "google-http-error");
        }

        var payload = await response.Content
            .ReadFromJsonAsync<GoogleSiteVerifyResponse>(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (payload is null)
        {
            return new ReCaptchaVerifyResult(false, "google-empty-response");
        }

        if (payload.Success)
        {
            return new ReCaptchaVerifyResult(true);
        }

        var code = payload.ErrorCodes is { Length: > 0 }
            ? string.Join(',', payload.ErrorCodes)
            : "google-rejected";
        return new ReCaptchaVerifyResult(false, code);
    }

    private void CleanupExpired()
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var pair in _challenges)
        {
            if (pair.Value.ExpiresAtUtc < now)
            {
                _challenges.TryRemove(pair.Key, out _);
            }
        }
    }

    private sealed record SimpleChallengeState(int ExpectedAnswer, DateTimeOffset ExpiresAtUtc);

    private sealed class GoogleSiteVerifyResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("error-codes")]
        public string[]? ErrorCodes { get; set; }
    }
}
