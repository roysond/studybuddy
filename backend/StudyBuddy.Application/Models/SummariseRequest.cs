namespace StudyBuddy.Application.Models;

/// <summary>
/// Request payload for the summarise endpoint.
/// </summary>
public sealed class SummariseRequest
{
    public required string StudyMaterial { get; init; }
}
