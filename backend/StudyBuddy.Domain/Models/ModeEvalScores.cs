namespace StudyBuddy.Domain.Models;

/// <summary>
/// Quality metric scores for a single tutoring mode: mode-level averages plus per-case detail.
/// </summary>
public sealed record ModeEvalScores(
    Dictionary<string, double> Scores,
    IReadOnlyList<EvalCaseResult> CaseResults);
