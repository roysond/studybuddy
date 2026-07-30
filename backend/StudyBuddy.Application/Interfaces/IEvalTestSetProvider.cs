using StudyBuddy.Application.Models;

namespace StudyBuddy.Application.Interfaces;

/// <summary>
/// Supplies the curated, per-mode eval fixtures used by <see cref="IEvalRunnerService"/>.
/// </summary>
public interface IEvalTestSetProvider
{
    IReadOnlyList<EvalTestCase> GetExplainCases();

    IReadOnlyList<EvalTestCase> GetQuizCases();

    IReadOnlyList<EvalTestCase> GetSummariseCases();
}
