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
    private long _totalSessionsEnded;
    private int _peakSessions;
    private long _totalDisconnects;
    private long _totalReconnects;
    private readonly DateTime _trackingStartedAt = DateTime.UtcNow;

    private DateTime _lastSnapshotTime = DateTime.UtcNow;
    private int _lastSnapshotCount;
    private SessionTrackingMode _trackingMode = SessionTrackingMode.Normal;

    public SessionMonitorService(IOptions<SessionMonitorOptions> options)
    {
        _options = options.Value;
    }

    internal void OnCircuitOpened(string circuitId, string? initialPath = null)
    {
        var session = new CircuitSession
        {
            CircuitId = circuitId,
            StartedAt = DateTime.UtcNow,
            CurrentPath = initialPath,
            CurrentPathUpdatedAt = initialPath is null ? null : DateTime.UtcNow
        };

        _activeSessions.TryAdd(circuitId, session);

        lock (_statsLock)
        {
            _totalSessionsStarted++;
            var currentCount = _activeSessions.Count;
            if (currentCount > _peakSessions)
            {
                _peakSessions = currentCount;
            }
        }

        UpdateTrackingMode(_activeSessions.Count);
        RecordSnapshot();
    }

    internal void OnCircuitClosed(string circuitId)
    {
        if (_activeSessions.TryRemove(circuitId, out var session))
        {
            session.EndedAt = DateTime.UtcNow;

            lock (_statsLock)
            {
                _totalSessionsEnded++;
            }

            UpdateTrackingMode(_activeSessions.Count);
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
        var currentCount = _activeSessions.Count;
        UpdateTrackingMode(currentCount);

        var sessions = _activeSessions.Values.ToList();
        var completedSessions = sessions.Where(s => s.EndedAt.HasValue).ToList();
        var disconnectedSessions = sessions.Where(s => s.DisconnectedAt.HasValue).ToList();

        double? avgDuration = null;
        if (completedSessions.Count > 0)
        {
            avgDuration = completedSessions
                .Average(s => (s.EndedAt!.Value - s.StartedAt).TotalSeconds);
        }

        return new SessionMetrics
        {
            ActiveSessions = currentCount,
            Timestamp = DateTime.UtcNow,
            PeakSessions = _peakSessions,
            TotalSessionsStarted = _totalSessionsStarted,
            TotalSessionsEnded = _totalSessionsEnded,
            AverageSessionDurationSeconds = avgDuration,
            DisconnectedSessions = disconnectedSessions.Count,
            TotalDisconnects = _totalDisconnects,
            TotalReconnects = _totalReconnects,
            TrackingSince = _trackingStartedAt,
            TrackingMode = _trackingMode,
            DegradedModeThreshold = _options.DegradedModeThreshold,
            EffectiveHistoryCap = GetEffectiveMaxHistorySize()
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
            .OrderByDescending(s => s.CurrentPathUpdatedAt ?? s.StartedAt)
            .Select(s => new ActiveCircuitSession
            {
                CircuitId = s.CircuitId,
                CurrentPath = s.CurrentPath,
                CurrentPathUpdatedAt = s.CurrentPathUpdatedAt,
                StartedAt = s.StartedAt,
                IsDisconnected = s.DisconnectedAt.HasValue
            })
            .ToList();
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

    private void RecordSnapshot()
    {
        var now = DateTime.UtcNow;
        var currentCount = _activeSessions.Count;

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

    private class CircuitSession
    {
        public string CircuitId { get; set; } = "";
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public DateTime? DisconnectedAt { get; set; }
        public TimeSpan? LastDisconnectDuration { get; set; }
        public string? CurrentPath { get; set; }
        public DateTime? CurrentPathUpdatedAt { get; set; }
    }
}
