namespace StudyBuddy.Infrastructure.ExternalServices;

/// <summary>
/// Marker/settings type for the OpenRouter OpenAI-compatible API.
/// Chat completion is wired through Semantic Kernel in the API host.
/// </summary>
public sealed class OpenRouterOptions
{
    public const string SectionName = "OpenRouter";

    public string BaseUrl { get; set; } = "https://openrouter.ai/api/v1";

    public string Model { get; set; } = "anthropic/claude-haiku-4-5";

    /// <summary>
    /// Prefer environment variable OPENROUTER_API_KEY over this value.
    /// </summary>
    public string? ApiKey { get; set; }
}
