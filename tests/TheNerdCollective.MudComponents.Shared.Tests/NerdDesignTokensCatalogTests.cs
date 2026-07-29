using Bunit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using MudBlazor;
using TheNerdCollective.Brand.Dnf;
using TheNerdCollective.MudComponents.DesignTokens;
using TheNerdCollective.MudComponents.ResponsiveTypography;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.Shared.Tests;

public class NerdDesignTokensCatalogTests : MudComponentTestContext
{
    [Fact]
    public void Catalog_shows_disabled_message_when_page_is_turned_off()
    {
        RegisterCatalogServices(new NerdDesignTokenOptions { EnableCatalogPage = false }, new NerdDesignTokenCss(string.Empty));

        var cut = Render<NerdDesignTokensCatalog>();

        Assert.Contains("disabled", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Catalog_renders_token_section_when_enabled_in_development()
    {
        var options = new NerdDesignTokenOptions { Prefix = "demo", EnableCatalogPage = true };
        options.Add("forest", new NerdColorToken { Value = "#365C3A", ContrastText = "#FFFFFF" });
        RegisterCatalogServices(options, new NerdDesignTokenCss(MudBlazorDesignTokenCssGenerator.Generate(options)));
        Services.AddScoped<NerdClipboardService>();

        var cut = Render(builder =>
        {
            builder.OpenComponent<MudBlazor.MudPopoverProvider>(0);
            builder.CloseComponent();
            builder.OpenComponent<NerdDesignTokensCatalog>(1);
            builder.CloseComponent();
        });

        Assert.Contains("Design token colors", cut.Markup);
        Assert.Contains("forest", cut.Markup);
    }

    [Fact]
    public void Recipes_catalog_renders_pairings_when_enabled_in_development()
    {
        NerdBrandPackRegistry.Instance.Reset();
        NerdBrandPackRegistry.Instance.Register(NerdDnfBrandPack.Instance);
        var options = new NerdDesignTokenOptions { EnableCatalogPage = true };
        NerdBrandPackRegistry.Instance.Configure("dnf", options);
        RegisterCatalogServices(options, new NerdDesignTokenCss(MudBlazorDesignTokenCssGenerator.Generate(options)), [NerdDnfBrandPack.Instance]);

        var cut = Render(builder =>
        {
            builder.OpenComponent<MudBlazor.MudPopoverProvider>(0);
            builder.CloseComponent();
            builder.OpenComponent<NerdDesignTokenRecipesCatalog>(1);
            builder.CloseComponent();
        });

        Assert.Contains("Design token recipes", cut.Markup);
        Assert.Contains("skov on kridt", cut.Markup, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("kridt-himmel", cut.Markup);
    }

    private void RegisterCatalogServices(
        NerdDesignTokenOptions options,
        NerdDesignTokenCss tokenCss,
        IEnumerable<INerdBrandPack>? brandPacks = null)
    {
        var hubOptions = new NerdDesignSystemOptions();
        Services.AddSingleton(options);
        Services.AddSingleton(tokenCss);
        Services.AddSingleton<INerdTokenPackStore>(new FakeTokenPackStore());
        Services.AddSingleton<INerdTokenCommentStore>(new FakeTokenCommentStore());
        Services.AddSingleton<INerdBrandPackSource>(new FakeBrandPackSource());
        Services.AddSingleton<INerdCatalogEntitlements, NerdOpenCatalogEntitlements>();
        Services.AddSingleton<INerdCatalogUpgradeUi, NerdDefaultCatalogUpgradeUi>();
        Services.AddSingleton<IEnumerable<INerdBrandPack>>(brandPacks ?? Array.Empty<INerdBrandPack>());
        Services.AddSingleton(hubOptions);
        Services.AddSingleton(new NerdResponsiveTypographyOptions());
        Services.AddSingleton(new NerdResponsiveTypographyCss(string.Empty));
        Services.AddSingleton<IWebHostEnvironment>(new TestHostEnvironment());
        Services.AddScoped<NerdDownloadService>();
        Services.AddSingleton<INerdMudThemeConfigurator, FakeThemeConfigurator>();
        Services.AddSingleton<INerdMudThemeController>(sp => new NerdMudThemeController(
            sp.GetRequiredService<NerdDesignTokenOptions>(),
            sp.GetRequiredService<NerdDesignTokenCss>(),
            sp.GetRequiredService<NerdDesignSystemOptions>(),
            sp.GetRequiredService<INerdMudThemeConfigurator>()));
        Services.AddSingleton<INerdBrandSwitcher, NerdBrandSwitcher>();
    }

    private sealed class FakeThemeConfigurator : INerdMudThemeConfigurator
    {
        public void Configure(MudTheme theme) => ArgumentNullException.ThrowIfNull(theme);

        public void OnBrandPackApplied(string brandPackId, MudTheme theme) => ArgumentNullException.ThrowIfNull(theme);
    }

    private sealed class TestHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Tests";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = "/";
        public string EnvironmentName { get; set; } = Environments.Development;
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = "/";
    }

    private sealed class FakeTokenPackStore : INerdTokenPackStore
    {
        public Task SaveAsync(NerdTokenPack pack, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<NerdTokenPack?> LoadAsync(string clientId, CancellationToken cancellationToken = default) =>
            Task.FromResult<NerdTokenPack?>(null);

        public Task<IReadOnlyList<string>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<string>>([]);
    }

    private sealed class FakeTokenCommentStore : INerdTokenCommentStore
    {
        public Task<IReadOnlyDictionary<string, string>> LoadAsync(
            string clientId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyDictionary<string, string>>(
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

        public Task SaveAsync(
            string clientId,
            IReadOnlyDictionary<string, string> comments,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class FakeBrandPackSource : INerdBrandPackSource
    {
        public bool IsConfigured => false;

        public string ClientId => "client";

        public string ExportDesignTokensJson() => "{}";

        public string ExportTypographyJson() => "{}";

        public void ApplyDesignTokensJson(string json)
        {
        }

        public void ApplyTypographyJson(string json)
        {
        }
    }
}
