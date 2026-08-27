using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace TheNerdCollective.Blazor.SessionMonitor;

/// <summary>
/// Implementation of session monitoring service.
/// </summary>
public class SessionMonitorService : ISessionMonitorService
{
    private readonly SessionMonitorOptions _options;
    private readonly ConcurrentDictionary<string, CircuitSession> _activeSessions = new();
    private readonly ConcurrentQueue<SessionSnapshot> _history = new();
    private readonly object _statsLock = new();

    private long _totalSessionsStarted;
    private long _totalCircuitsOpened;
    private long _totalSessionsEnded;
    private int _peakSessions;
    private long _totalDisconnects;
    private long _totalReconnects;
    private long _completedDurationTicks;
    private long _completedSessionCount;
    private readonly DateTime _trackingStartedAt = DateTime.UtcNow;

    private DateTime _lastSnapshotTime = DateTime.UtcNow;
    private int _lastSnapshotCount;
    private SessionTrackingMode _trackingMode = SessionTrackingMode.Normal;
    private readonly string? _instanceId;
    private readonly string _machineName;

    public SessionMonitorService(IOptions<SessionMonitorOptions> options)
    {
        _options = options.Value;
        _machineName = Environment.MachineName;
        _instanceId = ResolveInstanceId(_options.InstanceIdOverride);
    }

    internal void OnCircuitOpened(
        string circuitId,
        string? initialPath = null,
        string? clientId = null,
        SessionClientEnvironment? clientEnvironment = null)
    {
        var session = new CircuitSession
        {
            CircuitId = circuitId,
            ClientId = clientId,
            ClientEnvironment = clientEnvironment ?? SessionClientEnvironment.Unknown,
            StartedAt = DateTime.UtcNow,
            CurrentPath = initialPath,
            CurrentPathUpdatedAt = initialPath is null ? null : DateTime.UtcNow
        };

        _activeSessions.TryAdd(circuitId, session);

        lock (_statsLock)
        {
            _totalCircuitsOpened++;

            var isReloadReplacement = _options.DeduplicateReloadStarts
                && clientId is not null
                && _activeSessions.Values.Any(s =>
                    s.CircuitId != circuitId && s.ClientId == clientId);

            if (!isReloadReplacement)
            {
                _totalSessionsStarted++;
            }

            var connectedCount = GetConnectedSessionCount();
            if (connectedCount > _peakSessions)
            {
                _peakSessions = connectedCount;
            }
        }

        UpdateTrackingMode(GetConnectedSessionCount());
        RecordSnapshot();
    }

    internal void OnCircuitClosed(string circuitId)
    {
        if (_activeSessions.TryRemove(circuitId, out var session))
        {
            session.EndedAt = DateTime.UtcNow;
            var duration = session.EndedAt.Value - session.StartedAt;

            lock (_statsLock)
            {
                _totalSessionsEnded++;
                _completedDurationTicks += duration.Ticks;
                _completedSessionCount++;
            }

            UpdateTrackingMode(GetConnectedSessionCount());
            RecordSnapshot();
        }
    }

    internal void OnConnectionDown(string circuitId)
    {
        if (_activeSessions.TryGetValue(circuitId, out var session))
        {
            session.DisconnectedAt = DateTime.UtcNow;
            lock (_statsLock)
            {
                _totalDisconnects++;
            }
        }
    }

    internal void OnConnectionUp(string circuitId)
    {
        if (_activeSessions.TryGetValue(circuitId, out var session))
        {
            if (session.DisconnectedAt.HasValue)
            {
                var duration = DateTime.UtcNow - session.DisconnectedAt.Value;
                session.LastDisconnectDuration = duration;
                session.DisconnectedAt = null;
                lock (_statsLock)
                {
                    _totalReconnects++;
                }
            }
        }
    }

    public SessionMetrics GetCurrentMetrics()
    {
        var currentCount = GetConnectedSessionCount();
        UpdateTrackingMode(currentCount);

        var disconnectedSessions = _activeSessions.Values
            .Where(s => s.DisconnectedAt.HasValue)
            .ToList();

        double? avgDuration;
        lock (_statsLock)
        {
            avgDuration = _completedSessionCount > 0
                ? TimeSpan.FromTicks(_completedDurationTicks).TotalSeconds / _completedSessionCount
                : null;
        }

        return new SessionMetrics
        {
            ActiveSessions = currentCount,
            Timestamp = DateTime.UtcNow,
            PeakSessions = _peakSessions,
            TotalSessionsStarted = _totalSessionsStarted,
            TotalCircuitsOpened = _totalCircuitsOpened,
            TotalSessionsEnded = _totalSessionsEnded,
            AverageSessionDurationSeconds = avgDuration,
            DisconnectedSessions = disconnectedSessions.Count,
            TotalDisconnects = _totalDisconnects,
            TotalReconnects = _totalReconnects,
            TrackingSince = _trackingStartedAt,
            TrackingMode = _trackingMode,
            DegradedModeThreshold = _options.DegradedModeThreshold,
            EffectiveHistoryCap = GetEffectiveMaxHistorySize(),
            InstanceId = _instanceId,
            MachineName = _machineName
        };
    }

    public IEnumerable<SessionSnapshot> GetHistory(DateTime? since = null, int maxCount = 100)
    {
        if (IsDegradedMode())
        {
            maxCount = Math.Min(maxCount, _options.DegradedMaxHistorySize);
        }

        var snapshots = _history.ToArray();

        if (since.HasValue)
        {
            snapshots = snapshots.Where(s => s.Timestamp >= since.Value).ToArray();
        }

        return snapshots
            .OrderByDescending(s => s.Timestamp)
            .Take(maxCount);
    }

    public IEnumerable<string> GetActiveCircuitIds()
    {
        if (IsDegradedMode())
        {
            return Array.Empty<string>();
        }

        return _activeSessions.Keys.ToList();
    }

    public IEnumerable<ActiveCircuitSession> GetActiveSessions()
    {
        if (IsDegradedMode())
        {
            return Array.Empty<ActiveCircuitSession>();
        }

        return _activeSessions.Values
            .OrderBy(s => SessionMonitorService.FormatClientLabel(s.ClientId), StringComparer.OrdinalIgnoreCase)
            .ThenByDescending(s => s.CurrentPathUpdatedAt ?? s.StartedAt)
            .Select(s => MapToActiveCircuitSession(s))
            .ToList();
    }

    public IEnumerable<ActiveClientSessionSummary> GetActiveSessionsByClient()
    {
        var prefixes = _options.AdminMonitorPathPrefixes ?? [];
        var connected = _activeSessions.Values.Where(s => !s.DisconnectedAt.HasValue).ToList();
        var adminClientIds = GetAdminClientIds(connected, prefixes);

        var summaries = _activeSessions.Values
            .GroupBy(s => string.IsNullOrWhiteSpace(s.ClientId)
                ? ActiveClientSessionSummary.UnknownClientLabel
                : s.ClientId!)
            .Select(g =>
            {
                var circuits = g
                    .OrderByDescending(s => s.CurrentPathUpdatedAt ?? s.StartedAt)
                    .Select(s => MapToActiveCircuitSession(s, prefixes, adminClientIds))
                    .ToList();

                return new ActiveClientSessionSummary
                {
                    ClientId = g.Key == ActiveClientSessionSummary.UnknownClientLabel ? null : g.Key,
                    ClientLabel = FormatClientLabel(g.Key == ActiveClientSessionSummary.UnknownClientLabel ? null : g.Key),
                    ActiveSessionCount = circuits.Count,
                    ConnectedCount = circuits.Count(s => !s.IsDisconnected),
                    DisconnectedCount = circuits.Count(s => s.IsDisconnected),
                    DistinctPathCount = circuits
                        .Select(s => SessionPathNormalizer.GroupKey(s.CurrentPath))
                        .Distinct()
                        .Count(),
                    Circuits = circuits,
                    IsAdminMonitorGroup = IsAdminMonitorClientGroup(g, adminClientIds, prefixes),
                    ClientEnvironment = ResolveGroupClientEnvironment(g)
                };
            })
            .OrderByDescending(s => s.ActiveSessionCount)
            .ThenBy(s => s.ClientLabel, StringComparer.OrdinalIgnoreCase);

        if (IsDegradedMode())
        {
            return summaries.Take(_options.DegradedMaxPathSummaries).ToList();
        }

        return summaries.ToList();
    }

    public IEnumerable<ActivePathSessionSummary> GetActiveSessionsByPath()
    {
        var summaries = _activeSessions.Values
            .GroupBy(s => SessionPathNormalizer.GroupKey(s.CurrentPath))
            .Select(g => new ActivePathSessionSummary
            {
                Path = g.Key,
                ActiveSessionCount = g.Count(),
                ConnectedCount = g.Count(s => !s.DisconnectedAt.HasValue),
                DisconnectedCount = g.Count(s => s.DisconnectedAt.HasValue)
            })
            .OrderByDescending(s => s.ActiveSessionCount)
            .ThenBy(s => s.Path, StringComparer.OrdinalIgnoreCase);

        if (IsDegradedMode())
        {
            return summaries.Take(_options.DegradedMaxPathSummaries).ToList();
        }

        return summaries.ToList();
    }

    public SessionTrackingMode GetTrackingMode() => _trackingMode;

    internal void UpdateCurrentPath(string circuitId, string path)
    {
        if (_activeSessions.TryGetValue(circuitId, out var session))
        {
            session.CurrentPath = path;
            session.CurrentPathUpdatedAt = DateTime.UtcNow;
        }
    }

    public bool HasActiveSessions()
    {
        return !_activeSessions.IsEmpty;
    }

    public IEnumerable<DeploymentWindow> FindOptimalDeploymentWindows(int windowMinutes = 5, int lookbackHours = 24)
    {
        if (IsDegradedMode())
        {
            lookbackHours = Math.Min(lookbackHours, 6);
        }

        var since = DateTime.UtcNow.AddHours(-lookbackHours);
        var maxHistory = IsDegradedMode() ? _options.DegradedMaxHistorySize : int.MaxValue;
        var snapshots = GetHistory(since, maxHistory)
            .OrderBy(s => s.Timestamp)
            .ToList();

        if (snapshots.Count == 0)
        {
            return Array.Empty<DeploymentWindow>();
        }

        var windows = new List<DeploymentWindow>();
        var windowSpan = TimeSpan.FromMinutes(windowMinutes);

        var currentTime = snapshots.First().Timestamp;
        var endTime = snapshots.Last().Timestamp;

        while (currentTime.Add(windowSpan) <= endTime)
        {
            var windowEnd = currentTime.Add(windowSpan);
            var windowSnapshots = snapshots
                .Where(s => s.Timestamp >= currentTime && s.Timestamp < windowEnd)
                .ToList();

            if (windowSnapshots.Count > 0)
            {
                var maxSessions = windowSnapshots.Max(s => s.ActiveSessions);
                var avgSessions = windowSnapshots.Average(s => s.ActiveSessions);

                windows.Add(new DeploymentWindow
                {
                    StartTime = currentTime,
                    EndTime = windowEnd,
                    MaxActiveSessions = maxSessions,
                    AverageActiveSessions = avgSessions
                });
            }

            currentTime = currentTime.AddMinutes(1);
        }

        return windows
            .OrderBy(w => w.MaxActiveSessions)
            .ThenBy(w => w.AverageActiveSessions)
            .Take(20);
    }

    /// <inheritdoc />
    public DeploymentSafetyAssessment GetDeploymentSafety(int maxActiveSessions = 0)
    {
        var connected = _activeSessions.Values
            .Where(s => !s.DisconnectedAt.HasValue)
            .ToList();

        var prefixes = _options.AdminMonitorPathPrefixes ?? [];
        var adminClientIds = GetAdminClientIds(connected, prefixes);

        var adminMonitorSessions = connected.Count(s => IsExcludedForDeploySafety(s, adminClientIds, prefixes));
        var nonAdminActiveSessions = connected.Count - adminMonitorSessions;

        return new DeploymentSafetyAssessment
        {
            ActiveSessions = connected.Count,
            NonAdminActiveSessions = nonAdminActiveSessions,
            AdminMonitorSessions = adminMonitorSessions,
            HasAdminMonitorSession = connected.Any(s => SessionPathNormalizer.MatchesPathPrefix(s.CurrentPath, prefixes)),
            MaxActiveSessions = maxActiveSessions,
            CanDeploy = nonAdminActiveSessions <= maxActiveSessions
        };
    }

    private static HashSet<string> GetAdminClientIds(
        IReadOnlyList<CircuitSession> connected,
        IReadOnlyList<string> prefixes)
    {
        return connected
            .Where(s => !string.IsNullOrWhiteSpace(s.ClientId)
                && SessionPathNormalizer.MatchesPathPrefix(s.CurrentPath, prefixes))
            .Select(s => s.ClientId!)
            .ToHashSet(StringComparer.Ordinal);
    }

    private static bool IsAdminMonitorClientGroup(
        IEnumerable<CircuitSession> groupSessions,
        HashSet<string> adminClientIds,
        IReadOnlyList<string> prefixes)
    {
        var clientId = groupSessions.Select(s => s.ClientId).FirstOrDefault(id => !string.IsNullOrWhiteSpace(id));
        if (clientId is not null && adminClientIds.Contains(clientId))
        {
            return true;
        }

        return groupSessions.Any(s =>
            !s.DisconnectedAt.HasValue && SessionPathNormalizer.MatchesPathPrefix(s.CurrentPath, prefixes));
    }

    private static bool IsExcludedForDeploySafety(
        CircuitSession session,
        HashSet<string> adminClientIds,
        IReadOnlyList<string> prefixes)
    {
        if (SessionPathNormalizer.MatchesPathPrefix(session.CurrentPath, prefixes))
        {
            return true;
        }

        return session.ClientId is not null && adminClientIds.Contains(session.ClientId);
    }

    private void RecordSnapshot()
    {
        var now = DateTime.UtcNow;
        var currentCount = GetConnectedSessionCount();

        if (currentCount != _lastSnapshotCount || (now - _lastSnapshotTime).TotalMinutes >= 1)
        {
            var snapshot = new SessionSnapshot
            {
                Timestamp = now,
                ActiveSessions = currentCount,
                SessionsStarted = 0,
                SessionsEnded = 0
            };

            _history.Enqueue(snapshot);
            TrimHistoryTo(GetEffectiveMaxHistorySize());

            _lastSnapshotTime = now;
            _lastSnapshotCount = currentCount;
        }
    }

    private void UpdateTrackingMode(int activeCount)
    {
        var threshold = _options.DegradedModeThreshold;
        if (activeCount >= threshold && _trackingMode != SessionTrackingMode.DegradedSummaryOnly)
        {
            _trackingMode = SessionTrackingMode.DegradedSummaryOnly;
            TrimHistoryTo(_options.DegradedMaxHistorySize);
        }
        else if (activeCount < threshold && _trackingMode == SessionTrackingMode.DegradedSummaryOnly)
        {
            _trackingMode = SessionTrackingMode.Normal;
        }
    }

    private void TrimHistoryTo(int maxSize)
    {
        while (_history.Count > maxSize)
        {
            _history.TryDequeue(out _);
        }
    }

    private int GetEffectiveMaxHistorySize()
    {
        return IsDegradedMode()
            ? _options.DegradedMaxHistorySize
            : _options.NormalMaxHistorySize;
    }

    private bool IsDegradedMode() => _trackingMode == SessionTrackingMode.DegradedSummaryOnly;

    private int GetConnectedSessionCount() => _activeSessions.Values.Count(s => !s.DisconnectedAt.HasValue);

    private static string? ResolveInstanceId(string? instanceIdOverride)
    {
        if (!string.IsNullOrWhiteSpace(instanceIdOverride))
        {
            return instanceIdOverride;
        }

        return FirstNonEmptyEnvironmentVariable(
                   "CONTAINER_APP_REPLICA_NAME",
                   "WEBSITE_INSTANCE_ID",
                   "HOSTNAME")
               ?? Environment.MachineName;
    }

    private static string? FirstNonEmptyEnvironmentVariable(params string[] names)
    {
        foreach (var name in names)
        {
            var value = Environment.GetEnvironmentVariable(name);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }

    internal static string FormatClientLabel(string? clientId)
    {
        if (string.IsNullOrWhiteSpace(clientId))
        {
            return ActiveClientSessionSummary.UnknownClientLabel;
        }

        return clientId.Length <= 8 ? clientId : clientId[..8];
    }

    private static ActiveCircuitSession MapToActiveCircuitSession(
        CircuitSession session,
        IReadOnlyList<string>? prefixes = null,
        HashSet<string>? adminClientIds = null)
        => new()
        {
            CircuitId = session.CircuitId,
            ClientId = session.ClientId,
            ClientLabel = FormatClientLabel(session.ClientId),
            CurrentPath = session.CurrentPath,
            CurrentPathUpdatedAt = session.CurrentPathUpdatedAt,
            StartedAt = session.StartedAt,
            IsDisconnected = session.DisconnectedAt.HasValue,
            IsOnAdminMonitorPath = prefixes is not null
                && SessionPathNormalizer.MatchesPathPrefix(session.CurrentPath, prefixes),
            IsAdminMonitorGroup = prefixes is not null
                && adminClientIds is not null
                && IsAdminMonitorClientGroup([session], adminClientIds, prefixes),
            ClientEnvironment = session.ClientEnvironment
        };

    private static SessionClientEnvironment ResolveGroupClientEnvironment(IEnumerable<CircuitSession> groupSessions)
    {
        return groupSessions
            .Select(s => s.ClientEnvironment)
            .FirstOrDefault(env => !string.Equals(env.Browser, "Unknown", StringComparison.Ordinal)
                || !string.Equals(env.Platform, "Unknown", StringComparison.Ordinal)
                || env.IsAutomation)
            ?? groupSessions.Select(s => s.ClientEnvironment).FirstOrDefault()
            ?? SessionClientEnvironment.Unknown;
    }

    private class CircuitSession
    {
        public string CircuitId { get; set; } = "";
        public string? ClientId { get; set; }
        public SessionClientEnvironment ClientEnvironment { get; set; } = SessionClientEnvironment.Unknown;
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public DateTime? DisconnectedAt { get; set; }
        public TimeSpan? LastDisconnectDuration { get; set; }
        public string? CurrentPath { get; set; }
        public DateTime? CurrentPathUpdatedAt { get; set; }
    }
}
