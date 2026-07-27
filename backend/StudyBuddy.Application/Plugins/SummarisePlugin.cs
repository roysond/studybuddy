using System.ComponentModel;
using Microsoft.SemanticKernel;
using StudyBuddy.Application.Prompts;

namespace StudyBuddy.Application.Plugins;

/// <summary>
/// Semantic Kernel plugin that condenses study material into the 5 most
/// important points a student needs to remember.
/// </summary>
public sealed class SummarisePlugin
{
    public const string FunctionName = "Summarise";

    [KernelFunction(FunctionName)]
    [Description("Condenses study material into the 5 most important points a student needs to remember.")]
    public async Task<string> SummariseAsync(
        Kernel kernel,
        [Description("The study material to summarise")] string studyMaterial,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(kernel);
        ArgumentException.ThrowIfNullOrWhiteSpace(studyMaterial);

        var arguments = new KernelArguments
        {
            ["studyMaterial"] = studyMaterial
        };

        var result = await kernel.InvokePromptAsync(
            SummarisePromptTemplate.Template,
            arguments,
            cancellationToken: cancellationToken);

        return result.GetValue<string>() ?? string.Empty;
    }
}
