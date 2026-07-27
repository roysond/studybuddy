namespace StudyBuddy.Infrastructure.ExternalServices;

/// <summary>
/// Settings for the ElevenLabs text-to-speech API.
/// </summary>
public sealed class ElevenLabsOptions
{
    public const string SectionName = "ElevenLabs";

    public string BaseUrl { get; set; } = "https://api.elevenlabs.io/v1";

    public string? ApiKey { get; set; }

    public string? VoiceId { get; set; }

    public string ModelId { get; set; } = "eleven_multilingual_v2";
}
