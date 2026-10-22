using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using TheNerdCollective.Blazor.ReCaptcha;
using Xunit;

namespace TheNerdCollective.Blazor.ReCaptcha.Tests;

public sealed class NerdReCaptchaComponentTests
{
    [Fact]
    public async Task Activates_fallback_when_parameter_changes_after_initialization()
    {
        using var context = CreateContext();
        var cut = context.Render<NerdReCaptcha>();

        Assert.NotNull(cut.Find("[data-testid=nerd-recaptcha-enterprise]"));

        await cut.InvokeAsync(() => cut.Instance.SetParametersAsync(ParameterView.FromDictionary(
            new Dictionary<string, object?>
            {
                [nameof(NerdReCaptcha.UseSimpleFallback)] = true
            })));

        Assert.NotNull(cut.Find("[data-testid=nerd-recaptcha-simple]"));
        Assert.DoesNotContain("data-testid=\"nerd-recaptcha-enterprise\"", cut.Markup);
    }

    [Fact]
    public async Task Returns_math_token_without_executing_google_when_fallback_is_active()
    {
        using var context = CreateContext();
        var cut = context.Render<NerdReCaptcha>(parameters => parameters
            .Add(component => component.UseSimpleFallback, true));
        cut.Find("[data-testid=nerd-recaptcha-simple-answer]").Input("7");

        var token = await cut.InvokeAsync(cut.Instance.GenerateTokenAsync);

        Assert.Equal("simple:challenge-id:7", token);
        Assert.DoesNotContain(context.JSInterop.Invocations, invocation => invocation.Identifier == "execute");
    }

    private static TestContext CreateContext()
    {
        var context = new TestContext();
        context.Services.AddMudServices();
        context.Services.AddSingleton<INerdReCaptchaVerifier>(new StubVerifier());
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context;
    }

    private sealed class TestContext : BunitContext
    {
    }

    private sealed class StubVerifier : INerdReCaptchaVerifier
    {
        public NerdReCaptchaMode Mode => NerdReCaptchaMode.GoogleEnterprise;

        public string? SiteKey => null;

        public string? Action => "CONTACT";

        public Task<SimpleChallenge> CreateSimpleChallengeAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new SimpleChallenge("challenge-id", 3, 4, "3 + 4 = ?"));

        public Task<ReCaptchaVerifyResult> VerifyAsync(
            string? responseToken,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new ReCaptchaVerifyResult(true));
    }
}
