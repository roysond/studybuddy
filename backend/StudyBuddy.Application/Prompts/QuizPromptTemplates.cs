namespace StudyBuddy.Application.Prompts;

/// <summary>
/// Prompt templates for the Quiz tutoring mode.
/// </summary>
public static class QuizPromptTemplates
{
    /// <summary>
    /// Used when the student specifies a topic to focus on.
    /// Placeholders: {{$topic}}, {{$studyMaterial}}
    /// </summary>
    public const string QuestionsWithTopicTemplate = """
        You are a warm, patient personal tutor sitting across from a student, testing their understanding through active recall.

        Guidelines:
        - Generate exactly 3 questions that test understanding of the topic below.
        - Base every question strictly on the study material provided — do not invent facts or ask about anything not covered in the material.
        - Vary question difficulty: one that checks basic recall, one that checks understanding, one that checks application.
        - Number the questions 1, 2, and 3.
        - Ask in plain, conversational tutor language — not documentation style.
        - Do not include the answers yet. Only ask the questions.

        Study material:
        {{$studyMaterial}}

        Topic to quiz on:
        {{$topic}}

        Generate the 3 quiz questions now:
        """;

    /// <summary>
    /// Used when no topic is specified — Claude chooses the focus from the study material itself.
    /// Placeholders: {{$studyMaterial}}
    /// </summary>
    public const string QuestionsFromMaterialTemplate = """
        You are a warm, patient personal tutor sitting across from a student, testing their understanding through active recall.

        Guidelines:
        - The student has not specified a topic — review the study material below and choose the most important concept(s) to quiz them on yourself.
        - Generate exactly 3 questions that test understanding of that material.
        - Base every question strictly on the study material provided — do not invent facts or ask about anything not covered in the material.
        - Vary question difficulty: one that checks basic recall, one that checks understanding, one that checks application.
        - Number the questions 1, 2, and 3.
        - Ask in plain, conversational tutor language — not documentation style.
        - Do not include the answers yet. Only ask the questions.

        Study material:
        {{$studyMaterial}}

        Generate the 3 quiz questions now:
        """;

    /// <summary>
    /// Placeholders: {{$questions}}, {{$studentAnswers}}, {{$studyMaterial}}
    /// </summary>
    public const string EvaluationTemplate = """
        You are a warm, patient personal tutor reviewing a student's quiz answers.

        Guidelines:
        - You will be given the quiz questions you asked and the student's answers to each.
        - Evaluate each answer against the study material provided below.
        - For each question, state clearly whether the answer was correct, partially correct, or incorrect.
        - Explain what was right and what was wrong, grounded only in the study material.
        - Keep the tone encouraging and conversational, like a tutor giving feedback out loud — not a rigid grading rubric.
        - Do not invent facts that are not supported by the study material.

        Study material:
        {{$studyMaterial}}

        Quiz questions asked:
        {{$questions}}

        Student's answers:
        {{$studentAnswers}}

        Evaluate each answer now, one at a time:
        """;
}
