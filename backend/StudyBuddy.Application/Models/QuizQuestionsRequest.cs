namespace StudyBuddy.Application.Models;

/// <summary>
/// Request payload for generating quiz questions.
/// </summary>
public sealed class QuizQuestionsRequest
{
    public required string Topic { get; init; }

    public required string StudyMaterial { get; init; }
}
