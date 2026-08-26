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
        var clientId = GetOrCreateClientId(httpContext);

        _monitorService.OnCircuitOpened(circuit.Id, initialPath, clientId);
        return Task.CompletedTask;
    }

    private string? GetOrCreateClientId(HttpContext? httpContext)
    {
        if (httpContext is null)
        {
            return null;
        }

        var cookieName = _options.ClientIdCookieName;
        if (httpContext.Request.Cookies.TryGetValue(cookieName, out var existing)
            && !string.IsNullOrWhiteSpace(existing))
        {
            return existing;
        }

        var clientId = Guid.NewGuid().ToString("N");
        httpContext.Response.Cookies.Append(cookieName, clientId, new CookieOptions
        {
            HttpOnly = true,
            Secure = httpContext.Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            MaxAge = TimeSpan.FromDays(365),
            IsEssential = true
        });

        return clientId;
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
