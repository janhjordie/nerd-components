using Bunit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using TheNerdCollective.MudComponents.DesignTokens;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.Shared.Tests;

public class NerdDesignTokensCatalogTests : MudComponentTestContext
{
    [Fact]
    public void Catalog_shows_disabled_message_when_page_is_turned_off()
    {
        Services.AddSingleton(new NerdDesignTokenOptions { EnableCatalogPage = false });
        Services.AddSingleton<NerdDesignSystemOptions>(new NerdDesignSystemOptions());
        Services.AddSingleton<IWebHostEnvironment>(new TestHostEnvironment());
        Services.AddScoped<NerdDownloadService>();

        var cut = RenderComponent<NerdDesignTokensCatalog>();

        Assert.Contains("disabled", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Catalog_renders_token_section_when_enabled_in_development()
    {
        var options = new NerdDesignTokenOptions { Prefix = "demo", EnableCatalogPage = true };
        options.Add("forest", new NerdColorToken { Value = "#365C3A", ContrastText = "#FFFFFF" });

        Services.AddSingleton(options);
        Services.AddSingleton(new NerdDesignTokenCss(MudBlazorDesignTokenCssGenerator.Generate(options)));
        Services.AddSingleton<NerdDesignSystemOptions>(new NerdDesignSystemOptions());
        Services.AddSingleton<IWebHostEnvironment>(new TestHostEnvironment());
        Services.AddScoped<NerdDownloadService>();
        Services.AddScoped<NerdClipboardService>();

        var cut = Render(builder =>
        {
            builder.OpenComponent<MudBlazor.MudPopoverProvider>(0);
            builder.CloseComponent();
            builder.OpenComponent<NerdDesignTokensCatalog>(1);
            builder.CloseComponent();
        });

        Assert.Contains("Color tokens", cut.Markup);
        Assert.Contains("forest", cut.Markup);
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
}
