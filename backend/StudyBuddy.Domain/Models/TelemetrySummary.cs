namespace StudyBuddy.Domain.Models;

/// <summary>
/// Aggregated telemetry figures for the Developer Dashboard.
/// Tutoring fields exclude eval traffic; Eval* fields cover evaluation runs only.
/// </summary>
public sealed record TelemetrySummary(
    int CallsToday,
    double AverageLatencyMs,
    int TotalTokens,
    decimal EstimatedCostUsd,
    int EvalCallsToday,
    int EvalTotalTokens,
    decimal EvalEstimatedCostUsd);
