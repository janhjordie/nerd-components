using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using TheNerdCollective.Blazor.Observability;
using TheNerdCollective.MudComponents.ObservabilityDashboard;

namespace TheNerdCollective.MudComponents.ObservabilityDashboard.Tests;

public abstract class ObservabilityComponentTestContext : BunitContext
{
    protected ObservabilityComponentTestContext()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            DisposeAsync().AsTask().GetAwaiter().GetResult();
            return;
        }

        base.Dispose(disposing);
    }
}
