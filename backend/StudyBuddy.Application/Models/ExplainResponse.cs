namespace StudyBuddy.Application.Models;

/// <summary>
/// Response payload for the explain endpoint.
/// </summary>
public sealed class ExplainResponse
{
    public required string Explanation { get; init; }
}
