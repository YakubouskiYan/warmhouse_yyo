using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WarmhouseYyo.TelemetryService.Models;

[Table("readings", Schema = "telemetry_service")]
public class TelemetryReading
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    [Column("sensor_id")]
    public int SensorId { get; set; }

    [Required]
    [Column("value")]
    public double Value { get; set; }

    [Column("unit", TypeName = "varchar(20)")]
    public string? Unit { get; set; }

    [Column("recorded_at")]
    public DateTimeOffset RecordedAt { get; set; } = DateTimeOffset.UtcNow;
}
