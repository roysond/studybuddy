using Microsoft.SemanticKernel;
using StudyBuddy.Application.Interfaces;
using StudyBuddy.Application.Plugins;
using StudyBuddy.Domain.Models;

namespace StudyBuddy.Application.Services;

/// <summary>
/// Invokes the <see cref="SummarisePlugin"/> through the Semantic Kernel.
/// </summary>
public sealed class SummariseService : ISummariseService
{
    private readonly Kernel _kernel;

    public SummariseService(Kernel kernel)
    {
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
    }

    public async Task<SummariseResult> SummariseAsync(
        string studyMaterial,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(studyMaterial);

        var arguments = new KernelArguments
        {
            ["studyMaterial"] = studyMaterial
        };

        var result = await _kernel.InvokeAsync(
            pluginName: nameof(SummarisePlugin),
            functionName: SummarisePlugin.FunctionName,
            arguments,
            cancellationToken);

        var summary = result.GetValue<string>() ?? string.Empty;

        return new SummariseResult { Summary = summary };
    }
}
