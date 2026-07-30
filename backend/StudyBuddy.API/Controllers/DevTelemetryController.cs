using Microsoft.AspNetCore.Mvc;
using StudyBuddy.Application.Interfaces;
using StudyBuddy.Domain.Models;

namespace StudyBuddy.API.Controllers;

/// <summary>
/// Developer-facing telemetry endpoints. Decoupled from tutoring modes and from evals.
/// </summary>
[ApiController]
[Route("api/dev/telemetry")]
public sealed class DevTelemetryController : ControllerBase
{
    private readonly ITelemetryStore _telemetryStore;
    private readonly ILogger<DevTelemetryController> _logger;

    public DevTelemetryController(
        ITelemetryStore telemetryStore,
        ILogger<DevTelemetryController> logger)
    {
        _telemetryStore = telemetryStore ?? throw new ArgumentNullException(nameof(telemetryStore));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("recent")]
    [ProducesResponseType(typeof(IReadOnlyList<TelemetryEntry>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<TelemetryEntry>> GetRecent([FromQuery] int count = 20)
    {
        var clamped = Math.Clamp(count, 1, 200);
        _logger.LogDebug("Telemetry recent requested (count: {Count})", clamped);
        return Ok(_telemetryStore.GetRecent(clamped));
    }

    [HttpGet("summary")]
    [ProducesResponseType(typeof(TelemetrySummary), StatusCodes.Status200OK)]
    public ActionResult<TelemetrySummary> GetSummary()
    {
        return Ok(_telemetryStore.GetSummary());
    }
}
