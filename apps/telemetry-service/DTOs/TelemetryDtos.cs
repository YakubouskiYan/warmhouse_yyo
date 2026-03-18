using System.ComponentModel.DataAnnotations;

namespace WarmhouseYyo.TelemetryService.DTOs;

public record IngestReadingRequest(
    [Required] int SensorId,
    [Required] double Value,
    string? Unit
);

public record ReadingResponse(
    long Id,
    int SensorId,
    double Value,
    string? Unit,
    DateTimeOffset RecordedAt
);

public record HistoryResponse(
    int SensorId,
    IEnumerable<ReadingResponse> Readings,
    double? Average,
    double? Min,
    double? Max
);
