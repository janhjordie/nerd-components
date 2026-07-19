using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace TheNerdCollective.MudComponents.Shared.Tests;

public abstract class MudComponentTestContext : BunitContext
{
    protected MudComponentTestContext()
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
