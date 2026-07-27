using Microsoft.AspNetCore.Mvc;
using StudyBuddy.Application.Interfaces;
using StudyBuddy.Application.Models;

namespace StudyBuddy.API.Controllers;

[ApiController]
[Route("api/speech")]
public sealed class SpeechController : ControllerBase
{
    private readonly ISpeechService _speechService;
    private readonly ILogger<SpeechController> _logger;

    public SpeechController(
        ISpeechService speechService,
        ILogger<SpeechController> logger)
    {
        _speechService = speechService ?? throw new ArgumentNullException(nameof(speechService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Converts text into spoken audio using ElevenLabs.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Synthesise(
        [FromBody] SpeechRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest("Request body is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest($"{nameof(request.Text)} is required.");
        }

        _logger.LogInformation("Speech synthesis requested (text length: {Length})", request.Text.Length);

        var result = await _speechService.SynthesiseAsync(request.Text, cancellationToken);

        return File(result.Audio, result.ContentType);
    }
}
