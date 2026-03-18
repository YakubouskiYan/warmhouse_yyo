using Microsoft.AspNetCore.Mvc;
using WarmhouseYyo.TelemetryService.DTOs;
using WarmhouseYyo.TelemetryService.Services;

namespace WarmhouseYyo.TelemetryService.Controllers;

[ApiController]
[Route("api/v1/telemetry")]
public class TelemetryController(ITelemetryService service, ILogger<TelemetryController> logger)
    : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Ingest([FromBody] IngestReadingRequest request)
    {
        var reading = await service.IngestAsync(request);
        return CreatedAtAction(nameof(GetLatest), new { sensorId = reading.SensorId }, reading);
    }

    [HttpGet("{sensorId:int}/latest")]
    public async Task<IActionResult> GetLatest(int sensorId)
    {
        var result = await service.GetLatestAsync(sensorId);
        return result is null ? NotFound(new { error = $"No readings for sensor {sensorId}" }) : Ok(result);
    }

    [HttpGet("{sensorId:int}/history")]
    public async Task<IActionResult> GetHistory(
        int sensorId,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] int limit = 100)
    {
        if (limit is < 1 or > 1000) limit = 100;
        var result = await service.GetHistoryAsync(sensorId, from, to, limit);
        return Ok(result);
    }

    [HttpGet("/health")]
    public IActionResult Health() => Ok(new { status = "ok", service = "telemetry-service" });
}
