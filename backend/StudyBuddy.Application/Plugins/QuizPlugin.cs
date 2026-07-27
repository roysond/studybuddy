using System.ComponentModel;
using Microsoft.SemanticKernel;
using StudyBuddy.Application.Prompts;

namespace StudyBuddy.Application.Plugins;

/// <summary>
/// Semantic Kernel plugin that generates quiz questions from study material
/// and evaluates a student's answers against it.
/// </summary>
public sealed class QuizPlugin
{
    public const string GenerateQuestionsFunctionName = "GenerateQuestions";
    public const string EvaluateAnswersFunctionName = "EvaluateAnswers";

    [KernelFunction(GenerateQuestionsFunctionName)]
    [Description("Generates 3 quiz questions on a topic, grounded strictly in the loaded study material.")]
    public async Task<string> GenerateQuestionsAsync(
        Kernel kernel,
        [Description("The topic or section to quiz the student on")] string topic,
        [Description("The study material to ground the questions in")] string studyMaterial,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(kernel);
        ArgumentException.ThrowIfNullOrWhiteSpace(topic);
        ArgumentException.ThrowIfNullOrWhiteSpace(studyMaterial);

        var arguments = new KernelArguments
        {
            ["topic"] = topic,
            ["studyMaterial"] = studyMaterial
        };

        var result = await kernel.InvokePromptAsync(
            QuizPromptTemplates.QuestionsTemplate,
            arguments,
            cancellationToken: cancellationToken);

        return result.GetValue<string>() ?? string.Empty;
    }

    [KernelFunction(EvaluateAnswersFunctionName)]
    [Description("Evaluates a student's answers to quiz questions against the loaded study material and explains what was right or wrong.")]
    public async Task<string> EvaluateAnswersAsync(
        Kernel kernel,
        [Description("The quiz questions that were asked")] string questions,
        [Description("The student's answers to those questions")] string studentAnswers,
        [Description("The study material to evaluate the answers against")] string studyMaterial,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(kernel);
        ArgumentException.ThrowIfNullOrWhiteSpace(questions);
        ArgumentException.ThrowIfNullOrWhiteSpace(studentAnswers);
        ArgumentException.ThrowIfNullOrWhiteSpace(studyMaterial);

        var arguments = new KernelArguments
        {
            ["questions"] = questions,
            ["studentAnswers"] = studentAnswers,
            ["studyMaterial"] = studyMaterial
        };

        var result = await kernel.InvokePromptAsync(
            QuizPromptTemplates.EvaluationTemplate,
            arguments,
            cancellationToken: cancellationToken);

        return result.GetValue<string>() ?? string.Empty;
    }
}
