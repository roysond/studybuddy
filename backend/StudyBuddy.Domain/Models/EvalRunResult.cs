namespace StudyBuddy.Domain.Models;

/// <summary>
/// Result of an on-demand evaluation run across tutoring modes.
/// </summary>
public sealed record EvalRunResult(
    DateTimeOffset RunAt,
    Dictionary<string, ModeEvalScores> ModeScores);
