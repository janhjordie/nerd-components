namespace TheNerdCollective.Blazor.ReCaptcha;

/// <summary>
/// Configuration for Google reCAPTCHA v2. When site/secret keys are missing,
/// the package falls back to a simple math challenge automatically.
/// </summary>
public sealed class NerdReCaptchaOptions
{
    public const string SectionName = "NerdReCaptcha";

    /// <summary>Google reCAPTCHA v2 site key (public).</summary>
    public string? SiteKey { get; set; }

    /// <summary>Google reCAPTCHA v2 secret key (server-side).</summary>
    public string? SecretKey { get; set; }

    /// <summary>
    /// Optional explicit mode. When null, Google is used if both keys are set; otherwise Simple.
    /// </summary>
    public NerdReCaptchaMode? Mode { get; set; }

    /// <summary>How long simple challenges remain valid.</summary>
    public TimeSpan SimpleChallengeLifetime { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>Google siteverify endpoint (override only for tests).</summary>
    public string VerifyUrl { get; set; } = "https://www.google.com/recaptcha/api/siteverify";

    public bool HasGoogleKeys =>
        !string.IsNullOrWhiteSpace(SiteKey) && !string.IsNullOrWhiteSpace(SecretKey);

    public NerdReCaptchaMode ResolvedMode =>
        Mode ?? (HasGoogleKeys ? NerdReCaptchaMode.GoogleV2 : NerdReCaptchaMode.Simple);
}

public enum NerdReCaptchaMode
{
    GoogleV2,
    Simple
}
