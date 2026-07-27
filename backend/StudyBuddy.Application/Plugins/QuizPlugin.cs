using System.ComponentModel;
using Microsoft.SemanticKernel;
using StudyBuddy.Application.Prompts;

namespace StudyBuddy.Application.Plugins;

/// <summary>
/// Semantic Kernel plugin that generates quiz questions from study material
/// (on a given topic, or one chosen automatically) and evaluates a student's
/// answers against it.
/// </summary>
public sealed class QuizPlugin
{
    public const string GenerateQuestionsFunctionName = "GenerateQuestions";
    public const string EvaluateAnswersFunctionName = "EvaluateAnswers";

    [KernelFunction(GenerateQuestionsFunctionName)]
    [Description("Generates 3 quiz questions grounded in the loaded study material — on a given topic if specified, or on a topic chosen from the material itself if not.")]
    public async Task<string> GenerateQuestionsAsync(
        Kernel kernel,
        [Description("The study material to ground the questions in")] string studyMaterial,
        [Description("Optional. The topic or section to quiz the student on. If omitted, the topic is chosen automatically from the study material.")] string? topic = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(kernel);
        ArgumentException.ThrowIfNullOrWhiteSpace(studyMaterial);

        var hasTopic = !string.IsNullOrWhiteSpace(topic);

        var arguments = new KernelArguments
        {
            ["studyMaterial"] = studyMaterial
        };

        var template = QuizPromptTemplates.QuestionsFromMaterialTemplate;

        if (hasTopic)
        {
            arguments["topic"] = topic!;
            template = QuizPromptTemplates.QuestionsWithTopicTemplate;
        }

        var result = await kernel.InvokePromptAsync(
            template,
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
