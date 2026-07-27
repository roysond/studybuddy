namespace StudyBuddy.Application.Models;

/// <summary>
/// Request payload for the speech endpoint.
/// </summary>
public sealed class SpeechRequest
{
    public required string Text { get; init; }
}
