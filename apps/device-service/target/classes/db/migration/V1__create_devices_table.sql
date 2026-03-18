CREATE SCHEMA IF NOT EXISTS device_service;

CREATE TABLE IF NOT EXISTS device_service.devices (
    id               SERIAL PRIMARY KEY,
    name             VARCHAR(100) NOT NULL,
    serial_number    VARCHAR(100) NOT NULL UNIQUE,
    device_type      VARCHAR(50)  NOT NULL,
    home_id          VARCHAR(50),
    room_id          VARCHAR(50),
    firmware_version VARCHAR(20),
    status           VARCHAR(20)  NOT NULL DEFAULT 'offline',
    registered_at    TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_devices_home_id    ON device_service.devices (home_id);
CREATE INDEX IF NOT EXISTS idx_devices_device_type ON device_service.devices (device_type);
CREATE INDEX IF NOT EXISTS idx_devices_status      ON device_service.devices (status);
