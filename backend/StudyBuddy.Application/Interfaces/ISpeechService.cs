using StudyBuddy.Domain.Models;

namespace StudyBuddy.Application.Interfaces;

/// <summary>
/// Converts text into spoken audio. Implemented in Infrastructure —
/// the Application layer depends only on this abstraction.
/// </summary>
public interface ISpeechService
{
    Task<SpeechResult> SynthesiseAsync(
        string text,
        CancellationToken cancellationToken = default);
}
