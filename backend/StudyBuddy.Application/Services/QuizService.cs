using Microsoft.SemanticKernel;
using StudyBuddy.Application.Interfaces;
using StudyBuddy.Application.Plugins;
using StudyBuddy.Domain.Models;

namespace StudyBuddy.Application.Services;

/// <summary>
/// Invokes the <see cref="QuizPlugin"/> through the Semantic Kernel.
/// </summary>
public sealed class QuizService : IQuizService
{
    private readonly Kernel _kernel;

    public QuizService(Kernel kernel)
    {
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
    }

    public async Task<QuizQuestionsResult> GenerateQuestionsAsync(
        string studyMaterial,
        string? topic = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(studyMaterial);

        var arguments = new KernelArguments
        {
            ["studyMaterial"] = studyMaterial
        };

        if (!string.IsNullOrWhiteSpace(topic))
        {
            arguments["topic"] = topic;
        }

        var result = await _kernel.InvokeAsync(
            pluginName: nameof(QuizPlugin),
            functionName: QuizPlugin.GenerateQuestionsFunctionName,
            arguments,
            cancellationToken);

        var questions = result.GetValue<string>() ?? string.Empty;

        return new QuizQuestionsResult { Questions = questions };
    }

    public async Task<QuizEvaluationResult> EvaluateAnswersAsync(
        string questions,
        string studentAnswers,
        string studyMaterial,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(questions);
        ArgumentException.ThrowIfNullOrWhiteSpace(studentAnswers);
        ArgumentException.ThrowIfNullOrWhiteSpace(studyMaterial);

        var arguments = new KernelArguments
        {
            ["questions"] = questions,
            ["studentAnswers"] = studentAnswers,
            ["studyMaterial"] = studyMaterial
        };

        var result = await _kernel.InvokeAsync(
            pluginName: nameof(QuizPlugin),
            functionName: QuizPlugin.EvaluateAnswersFunctionName,
            arguments,
            cancellationToken);

        var evaluation = result.GetValue<string>() ?? string.Empty;

        return new QuizEvaluationResult { Evaluation = evaluation };
    }
}
