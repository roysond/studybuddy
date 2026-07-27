namespace StudyBuddy.Application.Models;

/// <summary>
/// Response payload containing quiz answer evaluation feedback.
/// </summary>
public sealed class QuizEvaluationResponse
{
    public required string Evaluation { get; init; }
}
