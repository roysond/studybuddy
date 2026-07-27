using System.ComponentModel;
using Microsoft.SemanticKernel;
using StudyBuddy.Application.Prompts;

namespace StudyBuddy.Application.Plugins;

/// <summary>
/// Semantic Kernel plugin that explains study material in plain, conversational
/// tutor language — either answering a specific question, or walking through
/// the full material if no question is given.
/// </summary>
public sealed class ExplainPlugin
{
    public const string FunctionName = "Explain";

    [KernelFunction(FunctionName)]
    [Description("Explains study material in plain, conversational tutor language — answers a specific student question if given, or explains the full material if not.")]
    public async Task<string> ExplainAsync(
        Kernel kernel,
        [Description("The study material to ground the explanation in")] string studyMaterial,
        [Description("Optional. The student's question or concept to explain. If omitted, the full study material is explained instead.")] string? userMessage = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(kernel);
        ArgumentException.ThrowIfNullOrWhiteSpace(studyMaterial);

        var hasQuestion = !string.IsNullOrWhiteSpace(userMessage);

        var arguments = new KernelArguments
        {
            ["studyMaterial"] = studyMaterial
        };

        var template = ExplainPromptTemplate.FullMaterialTemplate;

        if (hasQuestion)
        {
            arguments["userMessage"] = userMessage!;
            template = ExplainPromptTemplate.WithQuestionTemplate;
        }

        var result = await kernel.InvokePromptAsync(
            template,
            arguments,
            cancellationToken: cancellationToken);

        return result.GetValue<string>() ?? string.Empty;
    }
}
