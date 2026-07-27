using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using StudyBuddy.Application.Interfaces;
using StudyBuddy.Domain.Models;

namespace StudyBuddy.Infrastructure.ExternalServices;

/// <summary>
/// ElevenLabs implementation of <see cref="ISpeechService"/>.
/// Single responsibility: turn text into audio bytes via the ElevenLabs REST API.
/// </summary>
public sealed class ElevenLabsSpeechService : ISpeechService
{
    private const string AudioContentType = "audio/mpeg";
    private const string OutputFormat = "mp3_44100_128";

    private readonly HttpClient _httpClient;
    private readonly ElevenLabsOptions _options;

    public ElevenLabsSpeechService(
        IHttpClientFactory httpClientFactory,
        IOptions<ElevenLabsOptions> options)
    {
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(options);

        _httpClient = httpClientFactory.CreateClient("ElevenLabs");
        _options = options.Value;
    }

    public async Task<SpeechResult> SynthesiseAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        if (string.IsNullOrWhiteSpace(_options.VoiceId))
        {
            throw new InvalidOperationException(
                "ElevenLabs VoiceId is not configured. Set ELEVENLABS_VOICE_ID or ElevenLabs:VoiceId.");
        }

        var requestUri = $"text-to-speech/{_options.VoiceId}?output_format={OutputFormat}";

        var payload = new ElevenLabsSpeechRequest
        {
            Text = text,
            ModelId = _options.ModelId
        };

        using var response = await _httpClient.PostAsJsonAsync(requestUri, payload, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"ElevenLabs request failed with status {(int)response.StatusCode}: {error}");
        }

        var audio = await response.Content.ReadAsByteArrayAsync(cancellationToken);

        return new SpeechResult
        {
            Audio = audio,
            ContentType = AudioContentType
        };
    }

    private sealed class ElevenLabsSpeechRequest
    {
        [JsonPropertyName("text")]
        public required string Text { get; init; }

        [JsonPropertyName("model_id")]
        public required string ModelId { get; init; }
    }
}
