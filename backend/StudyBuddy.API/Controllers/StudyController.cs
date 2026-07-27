using Microsoft.AspNetCore.Mvc;
using StudyBuddy.Application.Interfaces;
using StudyBuddy.Application.Models;

namespace StudyBuddy.API.Controllers;

[ApiController]
[Route("api/study")]
public sealed class StudyController : ControllerBase
{
    private readonly IExplainService _explainService;
    private readonly IQuizService _quizService;
    private readonly ISummariseService _summariseService;
    private readonly ILogger<StudyController> _logger;

    public StudyController(
        IExplainService explainService,
        IQuizService quizService,
        ISummariseService summariseService,
        ILogger<StudyController> logger)
    {
        _explainService = explainService ?? throw new ArgumentNullException(nameof(explainService));
        _quizService = quizService ?? throw new ArgumentNullException(nameof(quizService));
        _summariseService = summariseService ?? throw new ArgumentNullException(nameof(summariseService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Explains a concept from the provided study material using Claude via Semantic Kernel.
    /// </summary>
    [HttpPost("explain")]
    [ProducesResponseType(typeof(ExplainResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ExplainResponse>> Explain(
        [FromBody] ExplainRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest("Request body is required.");
        }

        if (string.IsNullOrWhiteSpace(request.UserMessage))
        {
            return BadRequest($"{nameof(request.UserMessage)} is required.");
        }

        if (string.IsNullOrWhiteSpace(request.StudyMaterial))
        {
            return BadRequest($"{nameof(request.StudyMaterial)} is required.");
        }

        _logger.LogInformation("Explain request received (message length: {Length})", request.UserMessage.Length);

        var result = await _explainService.ExplainAsync(
            request.UserMessage,
            request.StudyMaterial,
            cancellationToken);

        return Ok(new ExplainResponse { Explanation = result.Explanation });
    }

    /// <summary>
    /// Generates quiz questions from the provided study material using Claude via Semantic Kernel.
    /// </summary>
    [HttpPost("quiz/questions")]
    [ProducesResponseType(typeof(QuizQuestionsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<QuizQuestionsResponse>> GenerateQuizQuestions(
        [FromBody] QuizQuestionsRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest("Request body is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Topic))
        {
            return BadRequest($"{nameof(request.Topic)} is required.");
        }

        if (string.IsNullOrWhiteSpace(request.StudyMaterial))
        {
            return BadRequest($"{nameof(request.StudyMaterial)} is required.");
        }

        _logger.LogInformation("Quiz question generation requested (topic length: {Length})", request.Topic.Length);

        var result = await _quizService.GenerateQuestionsAsync(
            request.Topic,
            request.StudyMaterial,
            cancellationToken);

        return Ok(new QuizQuestionsResponse { Questions = result.Questions });
    }

    /// <summary>
    /// Evaluates a student's quiz answers against the study material using Claude via Semantic Kernel.
    /// </summary>
    [HttpPost("quiz/evaluate")]
    [ProducesResponseType(typeof(QuizEvaluationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<QuizEvaluationResponse>> EvaluateQuizAnswers(
        [FromBody] QuizEvaluationRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest("Request body is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Questions))
        {
            return BadRequest($"{nameof(request.Questions)} is required.");
        }

        if (string.IsNullOrWhiteSpace(request.StudentAnswers))
        {
            return BadRequest($"{nameof(request.StudentAnswers)} is required.");
        }

        if (string.IsNullOrWhiteSpace(request.StudyMaterial))
        {
            return BadRequest($"{nameof(request.StudyMaterial)} is required.");
        }

        _logger.LogInformation("Quiz answer evaluation requested (answers length: {Length})", request.StudentAnswers.Length);

        var result = await _quizService.EvaluateAnswersAsync(
            request.Questions,
            request.StudentAnswers,
            request.StudyMaterial,
            cancellationToken);

        return Ok(new QuizEvaluationResponse { Evaluation = result.Evaluation });
    }

    /// <summary>
    /// Summarises study material into key points using Claude via Semantic Kernel.
    /// </summary>
    [HttpPost("summarise")]
    [ProducesResponseType(typeof(SummariseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SummariseResponse>> Summarise(
        [FromBody] SummariseRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest("Request body is required.");
        }

        if (string.IsNullOrWhiteSpace(request.StudyMaterial))
        {
            return BadRequest($"{nameof(request.StudyMaterial)} is required.");
        }

        _logger.LogInformation("Summarise request received (material length: {Length})", request.StudyMaterial.Length);

        var result = await _summariseService.SummariseAsync(
            request.StudyMaterial,
            cancellationToken);

        return Ok(new SummariseResponse { Summary = result.Summary });
    }
}
