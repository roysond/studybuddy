namespace StudyBuddy.Domain.Models;

/// <summary>
/// Origin of a captured Kernel call — real user traffic vs. an evaluation run.
/// </summary>
public static class TelemetrySource
{
    public const string Tutoring = "Tutoring";

    public const string Eval = "Eval";
}
