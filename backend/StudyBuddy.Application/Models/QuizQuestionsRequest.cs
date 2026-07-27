namespace StudyBuddy.Application.Models;

/// <summary>
/// Request payload for generating quiz questions.
/// </summary>
public sealed class QuizQuestionsRequest
{
    /// <summary>
    /// Optional. If omitted, the topic is chosen automatically from the study material.
    /// </summary>
    public string? Topic { get; init; }

    public required string StudyMaterial { get; init; }
}
