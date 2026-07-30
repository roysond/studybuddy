using Microsoft.AspNetCore.Mvc;
using StudyBuddy.Application.Interfaces;
using StudyBuddy.Domain.Models;

namespace StudyBuddy.API.Controllers;

/// <summary>
/// Developer-facing on-demand evaluation endpoints. Decoupled from live telemetry.
/// </summary>
[ApiController]
[Route("api/dev/evals")]
public sealed class DevEvalsController : ControllerBase
{
    private readonly IEvalRunnerService _evalRunnerService;
    private readonly IEvalResultStore _evalResultStore;
    private readonly ILogger<DevEvalsController> _logger;

    public DevEvalsController(
        IEvalRunnerService evalRunnerService,
        IEvalResultStore evalResultStore,
        ILogger<DevEvalsController> logger)
    {
        _evalRunnerService = evalRunnerService ?? throw new ArgumentNullException(nameof(evalRunnerService));
        _evalResultStore = evalResultStore ?? throw new ArgumentNullException(nameof(evalResultStore));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("run")]
    [ProducesResponseType(typeof(EvalRunResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<EvalRunResult>> Run(CancellationToken cancellationToken)
    {
        _logger.LogInformation("On-demand eval run started");

        var result = await _evalRunnerService.RunAsync(cancellationToken);
        _evalResultStore.Save(result);

        _logger.LogInformation("On-demand eval run completed at {RunAt}", result.RunAt);
        return Ok(result);
    }

    [HttpGet("latest")]
    [ProducesResponseType(typeof(EvalRunResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<EvalRunResult> GetLatest()
    {
        var latest = _evalResultStore.GetLatest();
        if (latest is null)
        {
            return NotFound("No eval run has been completed yet.");
        }

        return Ok(latest);
    }
}
