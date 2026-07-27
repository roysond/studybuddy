using Microsoft.SemanticKernel;
using StudyBuddy.Application.Interfaces;
using StudyBuddy.Application.Plugins;
using StudyBuddy.Domain.Models;

namespace StudyBuddy.Application.Services;

/// <summary>
/// Invokes the <see cref="ExplainPlugin"/> through the Semantic Kernel.
/// </summary>
public sealed class ExplainService : IExplainService
{
    private readonly Kernel _kernel;

    public ExplainService(Kernel kernel)
    {
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
    }

    public async Task<ExplainResult> ExplainAsync(
        string userMessage,
        string studyMaterial,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userMessage);
        ArgumentException.ThrowIfNullOrWhiteSpace(studyMaterial);

        var arguments = new KernelArguments
        {
            ["userMessage"] = userMessage,
            ["studyMaterial"] = studyMaterial
        };

        var result = await _kernel.InvokeAsync(
            pluginName: nameof(ExplainPlugin),
            functionName: ExplainPlugin.FunctionName,
            arguments,
            cancellationToken);

        var explanation = result.GetValue<string>() ?? string.Empty;

        return new ExplainResult { Explanation = explanation };
    }
}
