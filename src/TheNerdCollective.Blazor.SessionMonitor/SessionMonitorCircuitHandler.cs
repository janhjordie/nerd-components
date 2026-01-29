using Microsoft.AspNetCore.Components.Server.Circuits;

namespace TheNerdCollective.Blazor.SessionMonitor;

/// <summary>
/// Circuit handler that tracks session lifecycle events.
/// </summary>
public class SessionMonitorCircuitHandler : CircuitHandler
{
    private readonly SessionMonitorService _monitorService;

    public SessionMonitorCircuitHandler(ISessionMonitorService monitorService)
    {
        _monitorService = (SessionMonitorService)monitorService;
    }

    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _monitorService.OnCircuitOpened(circuit.Id);
        return Task.CompletedTask;
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _monitorService.OnCircuitClosed(circuit.Id);
        return Task.CompletedTask;
    }

    public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        // Connection down doesn't mean circuit closed - just temporary disconnect
        return Task.CompletedTask;
    }

    public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        // Connection restored
        return Task.CompletedTask;
    }
}
