using StudyBuddy.Domain.Models;

namespace StudyBuddy.Application.Interfaces;

/// <summary>
/// Application service for the Quiz tutoring mode.
/// </summary>
public interface IQuizService
{
    Task<QuizQuestionsResult> GenerateQuestionsAsync(
        string topic,
        string studyMaterial,
        CancellationToken cancellationToken = default);

    Task<QuizEvaluationResult> EvaluateAnswersAsync(
        string questions,
        string studentAnswers,
        string studyMaterial,
        CancellationToken cancellationToken = default);
}
