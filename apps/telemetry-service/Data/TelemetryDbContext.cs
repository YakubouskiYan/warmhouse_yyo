using Microsoft.EntityFrameworkCore;
using WarmhouseYyo.TelemetryService.Models;

namespace WarmhouseYyo.TelemetryService.Data;

public class TelemetryDbContext(DbContextOptions<TelemetryDbContext> options) : DbContext(options)
{
    public DbSet<TelemetryReading> Readings => Set<TelemetryReading>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("telemetry_service");

        modelBuilder.Entity<TelemetryReading>(entity =>
        {
            entity.HasIndex(r => r.SensorId).HasDatabaseName("idx_readings_sensor_id");
            entity.HasIndex(r => r.RecordedAt).HasDatabaseName("idx_readings_recorded_at").IsDescending();
        });
    }
}
