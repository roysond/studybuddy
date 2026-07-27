using StudyBuddy.Domain.Models;

namespace StudyBuddy.Application.Interfaces;

/// <summary>
/// Application service for the Explain tutoring mode.
/// </summary>
public interface IExplainService
{
    Task<ExplainResult> ExplainAsync(
        string studyMaterial,
        string? userMessage = null,
        CancellationToken cancellationToken = default);
}
