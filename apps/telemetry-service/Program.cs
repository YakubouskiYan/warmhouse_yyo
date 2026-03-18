using Microsoft.EntityFrameworkCore;
using WarmhouseYyo.TelemetryService.Data;
using WarmhouseYyo.TelemetryService.Messaging;
using WarmhouseYyo.TelemetryService.Services;

var builder = WebApplication.CreateBuilder(args);

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<TelemetryDbContext>(options =>
    options.UseNpgsql(connectionString));

// Services
builder.Services.AddScoped<ITelemetryService, TelemetryService>();

// RabbitMQ consumer (background service)
builder.Services.AddHostedService<TelemetryEventConsumer>();

// Web
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Auto-migrate on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TelemetryDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.MapControllers();

app.Run();
