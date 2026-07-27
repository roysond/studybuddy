namespace StudyBuddy.Domain.Models;

/// <summary>
/// Domain result produced when summarising study material into key points.
/// </summary>
public sealed class SummariseResult
{
    public required string Summary { get; init; }
}
