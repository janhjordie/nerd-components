using Microsoft.Extensions.DependencyInjection;
using TheNerdCollective.Blazor.ReCaptcha;
using Xunit;

namespace TheNerdCollective.Blazor.ReCaptcha.Tests;

public sealed class NerdReCaptchaVerifierTests
{
    [Fact]
    public void ResolvedMode_falls_back_to_simple_without_keys()
    {
        var services = new ServiceCollection();
        services.AddNerdReCaptcha(_ => { });
        using var sp = services.BuildServiceProvider();
        var verifier = sp.GetRequiredService<INerdReCaptchaVerifier>();

        Assert.Equal(NerdReCaptchaMode.Simple, verifier.Mode);
        Assert.Null(verifier.SiteKey);
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
}
