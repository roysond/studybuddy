namespace StudyBuddy.Application.Models;

/// <summary>
/// Request payload for evaluating a student's quiz answers.
/// </summary>
public sealed class QuizEvaluationRequest
{
    public required string Questions { get; init; }

    public required string StudentAnswers { get; init; }

    public required string StudyMaterial { get; init; }
}
