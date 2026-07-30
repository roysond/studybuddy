using StudyBuddy.Domain.Models;

namespace StudyBuddy.Application.Interfaces;

/// <summary>
/// Runs the on-demand quality evaluation suite against the tutoring services.
/// Fully decoupled from live telemetry capture.
/// </summary>
public interface IEvalRunnerService
{
    Task<EvalRunResult> RunAsync(CancellationToken cancellationToken = default);
}
