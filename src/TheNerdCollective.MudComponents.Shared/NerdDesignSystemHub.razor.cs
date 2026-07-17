using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace TheNerdCollective.MudComponents.Shared;

public partial class NerdDesignSystemHub
{
    [Inject]
    private NerdDesignSystemOptions Options { get; set; } = default!;

    [Inject]
    private IWebHostEnvironment HostEnvironment { get; set; } = default!;

    private bool IsAvailable =>
        Options.EnableHubPage &&
        (!Options.RestrictHubToDevelopment || HostEnvironment.IsDevelopment());
}
