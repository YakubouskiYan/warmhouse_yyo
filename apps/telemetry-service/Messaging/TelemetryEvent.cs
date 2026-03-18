namespace WarmhouseYyo.TelemetryService.Messaging;

public record TelemetryEvent(
    string EventType,
    int SensorId,
    double Value,
    string? Unit,
    string Timestamp
);
