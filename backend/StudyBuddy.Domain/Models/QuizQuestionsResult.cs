namespace StudyBuddy.Domain.Models;

/// <summary>
/// Domain result produced when generating quiz questions from study material.
/// </summary>
public sealed class QuizQuestionsResult
{
    public required string Questions { get; init; }
}
