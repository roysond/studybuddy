namespace StudyBuddy.Domain.Models;

/// <summary>
/// A single captured Semantic Kernel function invocation for developer telemetry.
/// </summary>
public sealed record TelemetryEntry(
    Guid Id,
    string Mode,
    int TokensIn,
    int TokensOut,
    long LatencyMs,
    DateTimeOffset Timestamp,
    string Source);
