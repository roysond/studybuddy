namespace StudyBuddy.Domain.Models;

/// <summary>
/// Domain result produced when converting text to speech.
/// </summary>
public sealed class SpeechResult
{
    public required byte[] Audio { get; init; }

    public required string ContentType { get; init; }
}
