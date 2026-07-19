using MudBlazor.Services;
using TheNerdCollective.Brand.Acme;
using TheNerdCollective.Brand.Demo;
using TheNerdCollective.Brand.Dnf;
using TheNerdCollective.Brand.Tnc;
using TheNerdCollective.Demo.Components;
using TheNerdCollective.MudComponents.DesignTokens;
using TheNerdCollective.MudComponents.ResponsiveTypography;
using TheNerdCollective.MudComponents.PlayBook;
using TheNerdCollective.MudComponents.Shared;
using TheNerdCollective.Services.BlazorServer;
using TheNerdCollective.Blazor.SessionMonitor;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBlazorServerCircuitServices(builder.Configuration, builder.Environment);
builder.Host.ConfigureBlazorServerCircuitShutdown();

// Add MudBlazor services
builder.Services.AddMudServices();

builder.Services.AddNerdDesignTokenBrandPacks(
    NerdDnfBrandPack.Instance,
    NerdAcmeBrandPack.Instance,
    NerdDemoBrandPack.Instance,
    NerdTncBrandPack.Instance);
builder.Services.AddNerdBrandTypographyPacks(
    NerdDnfBrandTypographyPack.Instance,
    NerdAcmeBrandTypographyPack.Instance,
    NerdDemoBrandTypographyPack.Instance,
    NerdTncBrandTypographyPack.Instance);
builder.Services.AddNerdDesignTokens(options =>
{
    options.RestrictCatalogToDevelopment = false;
    NerdBrandPackRegistry.Instance.Configure("tnc", options);
});
builder.Services.AddNerdDesignSystem(hub =>
{
    hub.RestrictWcagGuideToDevelopment = false;
});
builder.Services.AddNerdDesignTokenCatalog();

builder.Services.AddNerdResponsiveTypography(options =>
{
    options.RestrictCatalogToDevelopment = false;
    options.WarnOnAccessibilityFailuresAtStartup = false;
    NerdBrandTypographyRegistry.Instance.Configure("tnc", options);
});

builder.Services.AddNerdPlayBook(options =>
{
    options.RestrictPlayBookToDevelopment = false;
});

// Add session monitoring
builder.Services.AddSessionMonitoring();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
// Map reconnection status endpoint for the reconnection UI
app.MapBlazorReconnectionStatusEndpoint("/reconnection-status.json", async ctx =>
{
    var version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString();
    return await Task.FromResult(new ReconnectionStatus
    {
        Status = "ok",
        ReconnectingMessage = "Reconnecting...",
        Version = version
    });
});
// Map session monitoring endpoints
app.MapSessionMonitoringEndpoints();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddNerdDesignTokenCatalog(app.Services)
    .AddNerdResponsiveTypographyCatalog(app.Services)
    .AddNerdPlayBook(app.Services)
    .AddNerdDesignSystemHub(app.Services);

app.Run();
