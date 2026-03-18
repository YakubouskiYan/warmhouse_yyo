using WarmhouseYyo.TelemetryService.DTOs;
using WarmhouseYyo.TelemetryService.Models;

namespace WarmhouseYyo.TelemetryService.Services;

public interface ITelemetryService
{
    Task<TelemetryReading> IngestAsync(IngestReadingRequest request);
    Task<ReadingResponse?> GetLatestAsync(int sensorId);
    Task<HistoryResponse> GetHistoryAsync(int sensorId, DateTimeOffset? from, DateTimeOffset? to, int limit);
}
