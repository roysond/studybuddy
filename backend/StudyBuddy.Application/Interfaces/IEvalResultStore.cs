using StudyBuddy.Domain.Models;

namespace StudyBuddy.Application.Interfaces;

/// <summary>
/// Persists eval run results for the Developer Dashboard (latest + recent history).
/// </summary>
public interface IEvalResultStore
{
    void Save(EvalRunResult result);

    EvalRunResult? GetLatest();

    /// <summary>
    /// Returns up to <paramref name="count"/> most recent runs (newest first).
    /// </summary>
    IReadOnlyList<EvalRunResult> GetHistory(int count = 20);
}
