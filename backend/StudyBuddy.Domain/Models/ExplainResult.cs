namespace StudyBuddy.Domain.Models;

/// <summary>
/// Domain result produced by an explain tutoring interaction.
/// </summary>
public sealed class ExplainResult
{
    public required string Explanation { get; init; }
}
