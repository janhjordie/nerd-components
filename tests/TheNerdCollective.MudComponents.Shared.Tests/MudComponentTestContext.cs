using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace TheNerdCollective.MudComponents.Shared.Tests;

public abstract class MudComponentTestContext : TestContext
{
    protected MudComponentTestContext()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }
}
