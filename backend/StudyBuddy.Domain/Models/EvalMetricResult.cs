namespace StudyBuddy.Domain.Models;

/// <summary>
/// A single quality metric score plus optional evaluator reasoning.
/// </summary>
public sealed record EvalMetricResult(double Value, string? Reasoning);
