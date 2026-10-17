namespace TheNerdCollective.Blazor.ReCaptcha;

/// <summary>
/// Configuration for Google reCAPTCHA and the simple math-challenge fallback.
/// </summary>
public sealed class NerdReCaptchaOptions
{
    public const string SectionName = "NerdReCaptcha";

    /// <summary>Google reCAPTCHA site key (public).</summary>
    public string? SiteKey { get; set; }

    /// <summary>Google reCAPTCHA v2 secret key (server-side).</summary>
    public string? SecretKey { get; set; }

    /// <summary>Google Cloud project ID used to create reCAPTCHA Enterprise assessments.</summary>
    public string? ProjectId { get; set; }

    /// <summary>Restricted Google Cloud API key used by the server to create assessments.</summary>
    public string? ApiKey { get; set; }

    /// <summary>Expected action name for reCAPTCHA Enterprise score-based keys.</summary>
    public string Action { get; set; } = "submit";

    /// <summary>Minimum accepted reCAPTCHA Enterprise risk score.</summary>
    public float MinimumScore { get; set; } = 0.5f;

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

    public bool HasGoogleEnterpriseConfiguration =>
        !string.IsNullOrWhiteSpace(SiteKey)
        && !string.IsNullOrWhiteSpace(ProjectId)
        && !string.IsNullOrWhiteSpace(ApiKey);

    public NerdReCaptchaMode ResolvedMode =>
        Mode
        ?? (HasGoogleEnterpriseConfiguration
            ? NerdReCaptchaMode.GoogleEnterprise
            : HasGoogleKeys
                ? NerdReCaptchaMode.GoogleV2
                : NerdReCaptchaMode.Simple);
}

public enum NerdReCaptchaMode
{
    GoogleV2 = 0,
    Simple = 1,
    GoogleEnterprise = 2
}
