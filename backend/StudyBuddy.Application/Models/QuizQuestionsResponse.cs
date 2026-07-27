namespace StudyBuddy.Application.Models;

/// <summary>
/// Response payload containing generated quiz questions.
/// </summary>
public sealed class QuizQuestionsResponse
{
    public required string Questions { get; init; }
}
