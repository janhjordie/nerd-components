using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace TheNerdCollective.Blazor.SessionMonitor;

/// <summary>
/// Circuit handler that tracks session lifecycle events.
/// </summary>
public class SessionMonitorCircuitHandler : CircuitHandler
{
    private readonly SessionMonitorService _monitorService;
    private readonly SessionMonitorCircuitContext _circuitContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly SessionMonitorOptions _options;

    public SessionMonitorCircuitHandler(
        ISessionMonitorService monitorService,
        SessionMonitorCircuitContext circuitContext,
        IHttpContextAccessor httpContextAccessor,
        IOptions<SessionMonitorOptions> options)
    {
        _monitorService = (SessionMonitorService)monitorService;
        _circuitContext = circuitContext;
        _httpContextAccessor = httpContextAccessor;
        _options = options.Value;
    }

    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _circuitContext.CircuitId = circuit.Id;

        var httpContext = _httpContextAccessor.HttpContext;
        var initialPath = httpContext is null
            ? null
            : httpContext.Request.Path.Value + httpContext.Request.QueryString;
        var clientId = SessionMonitorClientIdResolver.Resolve(httpContext, _options.ClientIdCookieName);
        var clientEnvironment = SessionClientEnvironmentParser.Parse(httpContext);

        _monitorService.OnCircuitOpened(circuit.Id, initialPath, clientId, clientEnvironment);
        return Task.CompletedTask;
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        if (_circuitContext.CircuitId == circuit.Id)
        {
            _circuitContext.CircuitId = null;
        }

        _monitorService.OnCircuitClosed(circuit.Id);
        return Task.CompletedTask;
    }

    public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _monitorService.OnConnectionDown(circuit.Id);
        return Task.CompletedTask;
    }

    public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _monitorService.OnConnectionUp(circuit.Id);
        return Task.CompletedTask;
    }
}
