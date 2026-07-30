using Microsoft.Extensions.DependencyInjection;
using TheNerdCollective.Blazor.Observability;

namespace TheNerdCollective.Blazor.Observability.SigNoz.Tests;

public sealed class SigNozObservabilityServiceCollectionTests
{
    [Fact]
    public void AddObservabilityDashboardWithSigNoz_registers_backend_and_dashboard()
    {
        var services = new ServiceCollection();
        services.AddObservabilityDashboard(o => o.DefaultServiceName = "test-app");
        services.AddSigNozObservabilityBackend(o => o.BaseUrl = "http://127.0.0.1:8080");

        using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<IObservabilityDashboardService>());
        Assert.IsType<SigNozObservabilityBackend>(provider.GetRequiredService<IObservabilityBackend>());
    }
}
