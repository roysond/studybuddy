namespace StudyBuddy.Domain.Models;

/// <summary>
/// Per-test-case evaluation outcome: case name and metric scores with reasoning.
/// </summary>
public sealed record EvalCaseResult(
    string CaseName,
    Dictionary<string, EvalMetricResult> Metrics);
