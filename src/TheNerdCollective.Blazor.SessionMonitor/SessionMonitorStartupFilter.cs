using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace TheNerdCollective.Blazor.SessionMonitor;

/// <summary>
/// Registers <see cref="SessionMonitorClientIdMiddleware"/> for hosts that call <see cref="ServiceCollectionExtensions.AddSessionMonitoring"/>.
/// </summary>
internal sealed class SessionMonitorStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        => app =>
        {
            app.UseMiddleware<SessionMonitorClientIdMiddleware>();
            next(app);
        };
}
