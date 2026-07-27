namespace StudyBuddy.Infrastructure.ExternalServices;

/// <summary>
/// Settings placeholder for ElevenLabs TTS (integration deferred to a later phase).
/// </summary>
public sealed class ElevenLabsOptions
{
    public const string SectionName = "ElevenLabs";

    public string BaseUrl { get; set; } = "https://api.elevenlabs.io/v1";

    public string? ApiKey { get; set; }

    public string? VoiceId { get; set; }
}
