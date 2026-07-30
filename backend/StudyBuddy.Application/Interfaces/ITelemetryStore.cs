using StudyBuddy.Domain.Models;

namespace StudyBuddy.Application.Interfaces;

/// <summary>
/// Stores recent Kernel function telemetry for the Developer Dashboard.
/// Independent of the tutoring plugins and of the evaluation suite.
/// </summary>
public interface ITelemetryStore
{
    void Record(TelemetryEntry entry);

    IReadOnlyList<TelemetryEntry> GetRecent(int count);

    TelemetrySummary GetSummary();
}
