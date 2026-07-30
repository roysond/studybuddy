namespace StudyBuddy.Domain.Models;

/// <summary>
/// Quality metric scores for a single tutoring mode (metric name → score).
/// </summary>
public sealed record ModeEvalScores(Dictionary<string, double> Scores);
