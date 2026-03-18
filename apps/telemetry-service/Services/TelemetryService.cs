using Microsoft.EntityFrameworkCore;
using WarmhouseYyo.TelemetryService.Data;
using WarmhouseYyo.TelemetryService.DTOs;
using WarmhouseYyo.TelemetryService.Models;

namespace WarmhouseYyo.TelemetryService.Services;

public class TelemetryService(TelemetryDbContext db, ILogger<TelemetryService> logger)
    : ITelemetryService
{
    public async Task<TelemetryReading> IngestAsync(IngestReadingRequest request)
    {
        var reading = new TelemetryReading
        {
            SensorId = request.SensorId,
            Value = request.Value,
            Unit = request.Unit,
            RecordedAt = DateTimeOffset.UtcNow
        };

        db.Readings.Add(reading);
        await db.SaveChangesAsync();
        logger.LogInformation("Ingested reading for sensor {SensorId}: {Value} {Unit}", reading.SensorId, reading.Value, reading.Unit);
        return reading;
    }

    public async Task<ReadingResponse?> GetLatestAsync(int sensorId)
    {
        var reading = await db.Readings
            .Where(r => r.SensorId == sensorId)
            .OrderByDescending(r => r.RecordedAt)
            .FirstOrDefaultAsync();

        return reading is null ? null : ToResponse(reading);
    }

    public async Task<HistoryResponse> GetHistoryAsync(int sensorId, DateTimeOffset? from, DateTimeOffset? to, int limit)
    {
        var query = db.Readings.Where(r => r.SensorId == sensorId);

        if (from.HasValue) query = query.Where(r => r.RecordedAt >= from.Value);
        if (to.HasValue)   query = query.Where(r => r.RecordedAt <= to.Value);

        var readings = await query
            .OrderByDescending(r => r.RecordedAt)
            .Take(limit)
            .ToListAsync();

        double? avg = readings.Count > 0 ? readings.Average(r => r.Value) : null;
        double? min = readings.Count > 0 ? readings.Min(r => r.Value) : null;
        double? max = readings.Count > 0 ? readings.Max(r => r.Value) : null;

        return new HistoryResponse(sensorId, readings.Select(ToResponse), avg, min, max);
    }

    private static ReadingResponse ToResponse(TelemetryReading r) =>
        new(r.Id, r.SensorId, r.Value, r.Unit, r.RecordedAt);
}
