using StudyBuddy.Domain.Models;

namespace StudyBuddy.Application.Interfaces;

/// <summary>
/// Evaluates one named scenario and persists results for offline HTML reporting.
/// View reports with:
///   dotnet tool install --global Microsoft.Extensions.AI.Evaluation.Console
///   dotnet aieval report --path eval-reports --output report.html --open
/// </summary>
public interface IEvalReportWriter
{
    /// <summary>
    /// Runs the Quality evaluators for one scenario, writes the result under
    /// <c>eval-reports/</c> for <c>dotnet aieval report</c>, and returns display-name
    /// metrics (value + reasoning) for dashboard aggregation.
    /// </summary>
    Task<IReadOnlyDictionary<string, EvalMetricResult>> EvaluateAndPersistAsync(
        string scenarioName,
        string userPrompt,
        string modelOutput,
        string studyMaterialForGroundedness,
        CancellationToken cancellationToken = default);
}
