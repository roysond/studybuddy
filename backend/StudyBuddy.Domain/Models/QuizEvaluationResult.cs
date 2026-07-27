namespace StudyBuddy.Domain.Models;

/// <summary>
/// Domain result produced when evaluating a student's quiz answers.
/// </summary>
public sealed class QuizEvaluationResult
{
    public required string Evaluation { get; init; }
}
