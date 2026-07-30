using StudyBuddy.Application.Interfaces;

namespace StudyBuddy.Infrastructure.Evaluation;

/// <summary>
/// AsyncLocal implementation of <see cref="IEvalExecutionContext"/>. Uses a depth counter
/// so nested scopes behave correctly.
/// </summary>
public sealed class EvalExecutionContext : IEvalExecutionContext
{
    private static readonly AsyncLocal<int> Depth = new();

    public bool IsEvalRun => Depth.Value > 0;

    public IDisposable BeginEvalRun()
    {
        Depth.Value++;
        return new Scope();
    }

    private sealed class Scope : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            if (Depth.Value > 0)
            {
                Depth.Value--;
            }
        }
    }
}
