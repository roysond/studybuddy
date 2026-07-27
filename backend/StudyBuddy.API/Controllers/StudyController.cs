using Microsoft.AspNetCore.Mvc;
using StudyBuddy.Application.Interfaces;
using StudyBuddy.Application.Models;

namespace StudyBuddy.API.Controllers;

[ApiController]
[Route("api/study")]
public sealed class StudyController : ControllerBase
{
    private readonly IExplainService _explainService;
    private readonly ILogger<StudyController> _logger;

    public StudyController(IExplainService explainService, ILogger<StudyController> logger)
    {
        _explainService = explainService ?? throw new ArgumentNullException(nameof(explainService));
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
}
