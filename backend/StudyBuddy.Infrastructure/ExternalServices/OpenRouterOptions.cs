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

    /// <summary>
    /// USD per 1,000,000 input tokens. Used for dashboard cost estimates only — not billing-accurate.
    /// </summary>
    public decimal InputCostPerMillionUsd { get; set; } = 1.00m;

    /// <summary>
    /// USD per 1,000,000 output tokens. Used for dashboard cost estimates only — not billing-accurate.
    /// </summary>
    public decimal OutputCostPerMillionUsd { get; set; } = 5.00m;
}
