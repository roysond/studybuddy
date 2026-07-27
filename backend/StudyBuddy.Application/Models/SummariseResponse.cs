namespace StudyBuddy.Application.Models;

/// <summary>
/// Response payload for the summarise endpoint.
/// </summary>
public sealed class SummariseResponse
{
    public required string Summary { get; init; }
}
