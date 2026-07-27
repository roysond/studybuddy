namespace StudyBuddy.Domain.Entities;

/// <summary>
/// Represents study content loaded by the user for a tutoring session.
/// </summary>
public sealed class StudyMaterial
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string Title { get; set; }

    public required string Content { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
