using System.Collections.Generic;

namespace TheNerdCollective.Integrations.Dar.Models;

/// <summary>Decision trace for <see cref="Services.Dar.DarKommuneService.FindByCoordinatesWithDiagnosticsAsync"/>.</summary>
public sealed class KommuneCoordinateResolutionDiagnostics
{
    public double QueryLatitude { get; init; }

    public double QueryLongitude { get; init; }

    public string? ResolvedKommunekode { get; init; }

    public string? ResolvedNavn { get; init; }

    public string ResolutionPath { get; init; } = string.Empty;

    public string? PolygonMatchKommunekode { get; init; }

    public double? PolygonMatchReprDistanceMeters { get; init; }

    public string? NearestReprKommunekode { get; init; }

    public double? NearestReprDistanceMeters { get; init; }

    public bool RepresentativeProximityOverrideApplied { get; init; }

    public double RepresentativeProximitySnapMeters { get; init; }

    public int? GraphQlCandidateCount { get; init; }

    public IReadOnlyList<KommuneReprDistanceSnapshot> TopNearestRepresentativePoints { get; init; } =
        new List<KommuneReprDistanceSnapshot>();
}

public sealed class KommuneReprDistanceSnapshot
{
    public string? Kommunekode { get; init; }

    public string? Navn { get; init; }

    public double DistanceMeters { get; init; }
}
