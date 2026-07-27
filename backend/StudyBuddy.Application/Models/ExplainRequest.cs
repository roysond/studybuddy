namespace StudyBuddy.Application.Models;

/// <summary>
/// Request payload for the explain endpoint.
/// </summary>
public sealed class ExplainRequest
{
    /// <summary>
    /// Optional. If omitted, the full study material is explained instead of answering a specific question.
    /// </summary>
    public string? UserMessage { get; init; }

    public required string StudyMaterial { get; init; }
}
