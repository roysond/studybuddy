namespace StudyBuddy.Application.Models;

/// <summary>
/// One hardcoded eval fixture: study material plus an optional user message or quiz topic.
/// </summary>
public sealed record EvalTestCase(
    string Name,
    string StudyMaterial,
    string? UserMessageOrTopic = null);
