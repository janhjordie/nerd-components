using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text;
using TheNerdCollective.Blazor.ReCaptcha;
using Xunit;

namespace TheNerdCollective.Blazor.ReCaptcha.Tests;

public sealed class NerdReCaptchaVerifierTests
{
    [Fact]
    public async Task ResolvedMode_falls_back_to_simple_without_keys()
    {
        var services = new ServiceCollection();
        services.AddNerdReCaptcha(_ => { });
        using var sp = services.BuildServiceProvider();
        var verifier = sp.GetRequiredService<INerdReCaptchaVerifier>();

        Assert.Equal(NerdReCaptchaMode.Simple, verifier.Mode);
        Assert.Null(verifier.SiteKey);
        var challenge = await verifier.CreateSimpleChallengeAsync();
        Assert.StartsWith("Hvad er ", challenge.Prompt);
    }

    [Fact]
    public void Existing_simple_enum_value_and_config_remain_compatible()
    {
        Assert.Equal(1, (int)NerdReCaptchaMode.Simple);

        var services = new ServiceCollection();
        services.AddNerdReCaptcha(options =>
            options.Mode = (NerdReCaptchaMode)1);
        using var sp = services.BuildServiceProvider();

        Assert.Equal(
            NerdReCaptchaMode.Simple,
            sp.GetRequiredService<INerdReCaptchaVerifier>().Mode);
    }

    [Fact]
    public void Existing_verifier_implementations_can_omit_enterprise_action()
    {
        INerdReCaptchaVerifier verifier = new LegacyVerifier();

        Assert.Null(verifier.Action);
    }

    [Fact]
    public void Enterprise_configuration_selects_enterprise_mode()
    {
        var services = new ServiceCollection();
        services.AddNerdReCaptcha(options =>
        {
            options.SiteKey = "site-key";
            options.ProjectId = "project-id";
            options.ApiKey = "api-key";
            options.Action = "CONTACT";
        });
        using var sp = services.BuildServiceProvider();
        var verifier = sp.GetRequiredService<INerdReCaptchaVerifier>();

        Assert.Equal(NerdReCaptchaMode.GoogleEnterprise, verifier.Mode);
        Assert.Equal("site-key", verifier.SiteKey);
        Assert.Equal("CONTACT", verifier.Action);
    }

    [Fact]
    public async Task Enterprise_assessment_accepts_valid_token_with_matching_action_and_score()
    {
        var handler = new StubHttpMessageHandler("""
            {
              "tokenProperties": { "valid": true, "action": "CONTACT" },
              "riskAnalysis": { "score": 0.8 }
            }
            """);
        var verifier = CreateEnterpriseVerifier(handler);

        var result = await verifier.VerifyAsync("enterprise-token");

        Assert.True(result.Success);
        Assert.Equal(
            "https://recaptchaenterprise.googleapis.com/v1/projects/project-id/assessments?key=api-key",
            handler.RequestUri?.ToString());
        Assert.Contains("\"expectedAction\":\"CONTACT\"", handler.RequestBody);
        Assert.Contains("\"token\":\"enterprise-token\"", handler.RequestBody);
    }

    [Theory]
    [InlineData("""{"tokenProperties":{"valid":false,"action":"CONTACT"},"riskAnalysis":{"score":0.9}}""", "google-enterprise-invalid-token")]
    [InlineData("""{"tokenProperties":{"valid":true,"action":"OTHER"},"riskAnalysis":{"score":0.9}}""", "google-enterprise-action-mismatch")]
    [InlineData("""{"tokenProperties":{"valid":true,"action":"CONTACT"},"riskAnalysis":{"score":0.4}}""", "google-enterprise-score-too-low")]
    public async Task Enterprise_assessment_rejects_invalid_tokens(
        string assessment,
        string expectedError)
    {
        var verifier = CreateEnterpriseVerifier(new StubHttpMessageHandler(assessment));

        var result = await verifier.VerifyAsync("enterprise-token");

        Assert.False(result.Success);
        Assert.Equal(expectedError, result.ErrorCode);
    }

    [Fact]
    public async Task Simple_challenge_verifies_correct_answer()
    {
        var services = new ServiceCollection();
        services.AddNerdReCaptcha(options => options.Mode = NerdReCaptchaMode.Simple);
        using var sp = services.BuildServiceProvider();
        var verifier = sp.GetRequiredService<INerdReCaptchaVerifier>();

        var challenge = await verifier.CreateSimpleChallengeAsync();
        var token = $"simple:{challenge.ChallengeId}:{challenge.Left + challenge.Right}";
        var result = await verifier.VerifyAsync(token);

        Assert.True(result.Success);
    }

    [Fact]
    public async Task Simple_challenge_rejects_wrong_answer_and_is_single_use()
    {
        var services = new ServiceCollection();
        services.AddNerdReCaptcha(options => options.Mode = NerdReCaptchaMode.Simple);
        using var sp = services.BuildServiceProvider();
        var verifier = sp.GetRequiredService<INerdReCaptchaVerifier>();

        var challenge = await verifier.CreateSimpleChallengeAsync();
        var wrong = await verifier.VerifyAsync($"simple:{challenge.ChallengeId}:999");
        Assert.False(wrong.Success);

        var challenge2 = await verifier.CreateSimpleChallengeAsync();
        var ok = await verifier.VerifyAsync($"simple:{challenge2.ChallengeId}:{challenge2.Left + challenge2.Right}");
        Assert.True(ok.Success);
        var reuse = await verifier.VerifyAsync($"simple:{challenge2.ChallengeId}:{challenge2.Left + challenge2.Right}");
        Assert.False(reuse.Success);
    }

    [Theory]
    [InlineData(NerdReCaptchaMode.GoogleV2)]
    [InlineData(NerdReCaptchaMode.GoogleEnterprise)]
    public async Task Simple_fallback_token_is_accepted_when_google_mode_is_configured(
        NerdReCaptchaMode mode)
    {
        var services = new ServiceCollection();
        services.AddNerdReCaptcha(options =>
        {
            options.Mode = mode;
            options.SiteKey = "site-key";
            options.SecretKey = "secret-key";
            options.ProjectId = "project-id";
            options.ApiKey = "api-key";
        });
        services.AddHttpClient(nameof(NerdReCaptchaVerifier))
            .ConfigurePrimaryHttpMessageHandler(() => new StubHttpMessageHandler("{}"));
        using var sp = services.BuildServiceProvider();
        var verifier = sp.GetRequiredService<INerdReCaptchaVerifier>();

        var challenge = await verifier.CreateSimpleChallengeAsync();
        var token = $"simple:{challenge.ChallengeId}:{challenge.Left + challenge.Right}";

        var result = await verifier.VerifyAsync(token);

        Assert.True(result.Success);
    }

    private static INerdReCaptchaVerifier CreateEnterpriseVerifier(StubHttpMessageHandler handler)
    {
        var services = new ServiceCollection();
        services.AddNerdReCaptcha(options =>
        {
            options.Mode = NerdReCaptchaMode.GoogleEnterprise;
            options.SiteKey = "site-key";
            options.ProjectId = "project-id";
            options.ApiKey = "api-key";
            options.Action = "CONTACT";
        });
        services.AddHttpClient(nameof(NerdReCaptchaVerifier))
            .ConfigurePrimaryHttpMessageHandler(() => handler);
        return services.BuildServiceProvider().GetRequiredService<INerdReCaptchaVerifier>();
    }

    private sealed class StubHttpMessageHandler(string responseBody) : HttpMessageHandler
    {
        public Uri? RequestUri { get; private set; }
        public string RequestBody { get; private set; } = "";

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;
            RequestBody = await request.Content!.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
            };
        }
    }

    private sealed class LegacyVerifier : INerdReCaptchaVerifier
    {
        public NerdReCaptchaMode Mode => NerdReCaptchaMode.Simple;
        public string? SiteKey => null;

        public Task<SimpleChallenge> CreateSimpleChallengeAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new SimpleChallenge("id", 1, 1, "Hvad er 1 + 1?"));

        public Task<ReCaptchaVerifyResult> VerifyAsync(
            string? responseToken,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new ReCaptchaVerifyResult(true));
    }
}
