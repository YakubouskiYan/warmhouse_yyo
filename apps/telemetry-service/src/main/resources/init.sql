CREATE SCHEMA IF NOT EXISTS telemetry_service;

CREATE TABLE IF NOT EXISTS telemetry_service.readings (
    id          BIGSERIAL PRIMARY KEY,
    sensor_id   INTEGER          NOT NULL,
    value       DOUBLE PRECISION NOT NULL,
    unit        VARCHAR(20),
    recorded_at TIMESTAMPTZ      NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_readings_sensor_id   ON telemetry_service.readings (sensor_id);
CREATE INDEX IF NOT EXISTS idx_readings_recorded_at ON telemetry_service.readings (recorded_at DESC);
