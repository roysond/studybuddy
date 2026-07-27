using StudyBuddy.Domain.Models;

namespace StudyBuddy.Application.Interfaces;

/// <summary>
/// Application service for the Summarise tutoring mode.
/// </summary>
public interface ISummariseService
{
    Task<SummariseResult> SummariseAsync(
        string studyMaterial,
        CancellationToken cancellationToken = default);
}
