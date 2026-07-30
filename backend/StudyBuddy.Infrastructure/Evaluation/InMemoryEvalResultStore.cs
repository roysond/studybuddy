using StudyBuddy.Application.Interfaces;
using StudyBuddy.Domain.Models;

namespace StudyBuddy.Infrastructure.Evaluation;

/// <summary>
/// Thread-safe in-memory store that keeps only the latest eval run result.
/// </summary>
public sealed class InMemoryEvalResultStore : IEvalResultStore
{
    private readonly object _gate = new();
    private EvalRunResult? _latest;

    public void Save(EvalRunResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        lock (_gate)
        {
            _latest = result;
        }
    }

    public EvalRunResult? GetLatest()
    {
        lock (_gate)
        {
            return _latest;
        }
    }
}
