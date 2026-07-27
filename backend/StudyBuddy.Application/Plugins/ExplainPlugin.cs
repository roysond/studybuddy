using System.ComponentModel;
using Microsoft.SemanticKernel;
using StudyBuddy.Application.Prompts;

namespace StudyBuddy.Application.Plugins;

/// <summary>
/// Semantic Kernel plugin that explains concepts from study material
/// in plain, conversational tutor language.
/// </summary>
public sealed class ExplainPlugin
{
    public const string FunctionName = "Explain";

    [KernelFunction(FunctionName)]
    [Description("Explains a concept from the loaded study material in plain, conversational tutor language — not documentation style.")]
    public async Task<string> ExplainAsync(
        Kernel kernel,
        [Description("The student's question or concept to explain")] string userMessage,
        [Description("The study material to ground the explanation in")] string studyMaterial,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(kernel);
        ArgumentException.ThrowIfNullOrWhiteSpace(userMessage);
        ArgumentException.ThrowIfNullOrWhiteSpace(studyMaterial);

        var arguments = new KernelArguments
        {
            ["userMessage"] = userMessage,
            ["studyMaterial"] = studyMaterial
        };

        var result = await kernel.InvokePromptAsync(
            ExplainPromptTemplate.Template,
            arguments,
            cancellationToken: cancellationToken);

        return result.GetValue<string>() ?? string.Empty;
    }
}
