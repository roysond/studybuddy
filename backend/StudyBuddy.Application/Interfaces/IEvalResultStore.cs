using StudyBuddy.Domain.Models;

namespace StudyBuddy.Application.Interfaces;

/// <summary>
/// Persists the latest eval run result for the Developer Dashboard.
/// </summary>
public interface IEvalResultStore
{
    void Save(EvalRunResult result);

    EvalRunResult? GetLatest();
}
