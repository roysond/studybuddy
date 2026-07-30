namespace StudyBuddy.Application.Interfaces;

/// <summary>
/// Ambient marker indicating whether the current async flow is part of an evaluation run.
/// Lets telemetry distinguish eval-generated Kernel calls from real user traffic without
/// coupling the tutoring services to the evaluation suite.
/// </summary>
public interface IEvalExecutionContext
{
    bool IsEvalRun { get; }

    /// <summary>
    /// Marks the current async flow as an eval run until the returned scope is disposed.
    /// Safe to nest.
    /// </summary>
    IDisposable BeginEvalRun();
}
