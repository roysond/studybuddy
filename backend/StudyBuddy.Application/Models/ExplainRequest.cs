namespace StudyBuddy.Application.Models;

/// <summary>
/// Request payload for the explain endpoint.
/// </summary>
public sealed class ExplainRequest
{
    public required string UserMessage { get; init; }

    public required string StudyMaterial { get; init; }
}
