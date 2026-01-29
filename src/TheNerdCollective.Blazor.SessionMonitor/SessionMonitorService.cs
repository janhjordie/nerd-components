using Microsoft.AspNetCore.Components.Server.Circuits;
using System.Collections.Concurrent;

namespace TheNerdCollective.Blazor.SessionMonitor;

/// <summary>
/// Implementation of session monitoring service.
/// </summary>
public class SessionMonitorService : ISessionMonitorService
{
    private readonly ConcurrentDictionary<string, CircuitSession> _activeSessions = new();
    private readonly ConcurrentQueue<SessionSnapshot> _history = new();
    private readonly object _statsLock = new();
    
    private long _totalSessionsStarted;
    private long _totalSessionsEnded;
    private int _peakSessions;
    private readonly DateTime _trackingStartedAt = DateTime.UtcNow;
    
    private const int MaxHistorySize = 10000; // Keep last 10k snapshots
    private DateTime _lastSnapshotTime = DateTime.UtcNow;
    private int _lastSnapshotCount;

    internal void OnCircuitOpened(string circuitId)
    {
        var session = new CircuitSession
        {
            CircuitId = circuitId,
            StartedAt = DateTime.UtcNow
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

            RecordSnapshot();
        }
    }

    public SessionMetrics GetCurrentMetrics()
    {
        var currentCount = _activeSessions.Count;
        var completedSessions = _activeSessions.Values
            .Where(s => s.EndedAt.HasValue)
            .ToList();

        double? avgDuration = null;
        if (completedSessions.Any())
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
            AverageSessionDurationSeconds = avgDuration
        };
    }

    public IEnumerable<SessionSnapshot> GetHistory(DateTime? since = null, int maxCount = 100)
    {
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
        return _activeSessions.Keys.ToList();
    }

    public bool HasActiveSessions()
    {
        return !_activeSessions.IsEmpty;
    }

    public IEnumerable<DeploymentWindow> FindOptimalDeploymentWindows(int windowMinutes = 5, int lookbackHours = 24)
    {
        var since = DateTime.UtcNow.AddHours(-lookbackHours);
        var snapshots = GetHistory(since, int.MaxValue)
            .OrderBy(s => s.Timestamp)
            .ToList();

        if (!snapshots.Any())
        {
            return Array.Empty<DeploymentWindow>();
        }

        var windows = new List<DeploymentWindow>();
        var windowSpan = TimeSpan.FromMinutes(windowMinutes);

        // Group snapshots into windows
        var currentTime = snapshots.First().Timestamp;
        var endTime = snapshots.Last().Timestamp;

        while (currentTime.Add(windowSpan) <= endTime)
        {
            var windowEnd = currentTime.Add(windowSpan);
            var windowSnapshots = snapshots
                .Where(s => s.Timestamp >= currentTime && s.Timestamp < windowEnd)
                .ToList();

            if (windowSnapshots.Any())
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

            currentTime = currentTime.AddMinutes(1); // Slide window by 1 minute
        }

        // Return windows sorted by best deployment time (zero sessions first, then lowest max)
        return windows
            .OrderBy(w => w.MaxActiveSessions)
            .ThenBy(w => w.AverageActiveSessions)
            .Take(20);
    }

    private void RecordSnapshot()
    {
        var now = DateTime.UtcNow;
        var currentCount = _activeSessions.Count;

        // Only record if count changed or 1 minute has passed
        if (currentCount != _lastSnapshotCount || (now - _lastSnapshotTime).TotalMinutes >= 1)
        {
            var snapshot = new SessionSnapshot
            {
                Timestamp = now,
                ActiveSessions = currentCount,
                SessionsStarted = 0, // Could track delta if needed
                SessionsEnded = 0
            };

            _history.Enqueue(snapshot);

            // Trim history if too large
            while (_history.Count > MaxHistorySize)
            {
                _history.TryDequeue(out _);
            }

            _lastSnapshotTime = now;
            _lastSnapshotCount = currentCount;
        }
    }

    private class CircuitSession
    {
        public string CircuitId { get; set; } = "";
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
    }
}
