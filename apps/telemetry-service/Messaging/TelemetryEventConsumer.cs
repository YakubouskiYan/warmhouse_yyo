using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using WarmhouseYyo.TelemetryService.DTOs;
using WarmhouseYyo.TelemetryService.Services;

namespace WarmhouseYyo.TelemetryService.Messaging;

public class TelemetryEventConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TelemetryEventConsumer> _logger;
    private readonly IConfiguration _config;
    private IConnection? _connection;
    private IModel? _channel;

    public TelemetryEventConsumer(
        IServiceScopeFactory scopeFactory,
        ILogger<TelemetryEventConsumer> logger,
        IConfiguration config)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ConnectWithRetryAsync(stoppingToken);

        if (_channel is null) return;

        var exchange = _config["RabbitMQ:Exchange"] ?? "warmhouse.events";
        var queue = _config["RabbitMQ:TelemetryQueue"] ?? "telemetry.readings.queue";

        _channel.ExchangeDeclare(exchange, ExchangeType.Topic, durable: true);
        _channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(queue, exchange, "telemetry.reading");

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) => await HandleMessageAsync(ea);

        _channel.BasicConsume(queue, autoAck: false, consumer: consumer);
        _logger.LogInformation("TelemetryEventConsumer listening on queue '{Queue}'", queue);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task HandleMessageAsync(BasicDeliverEventArgs ea)
    {
        try
        {
            var body = Encoding.UTF8.GetString(ea.Body.ToArray());
            var evt = JsonSerializer.Deserialize<TelemetryEvent>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (evt is null)
            {
                _logger.LogWarning("Received null or invalid telemetry event");
                _channel?.BasicAck(ea.DeliveryTag, false);
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<ITelemetryService>();
            await svc.IngestAsync(new IngestReadingRequest(evt.SensorId, evt.Value, evt.Unit));

            _channel?.BasicAck(ea.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing telemetry event");
            _channel?.BasicNack(ea.DeliveryTag, false, requeue: true);
        }
    }

    private async Task ConnectWithRetryAsync(CancellationToken ct)
    {
        var host = _config["RabbitMQ:Host"] ?? "rabbitmq";
        var port = int.Parse(_config["RabbitMQ:Port"] ?? "5672");
        var user = _config["RabbitMQ:Username"] ?? "guest";
        var pass = _config["RabbitMQ:Password"] ?? "guest";

        var factory = new ConnectionFactory
        {
            HostName = host,
            Port = port,
            UserName = user,
            Password = pass
        };

        const int maxAttempts = 10;
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                _logger.LogInformation("Connected to RabbitMQ at {Host}:{Port}", host, port);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("RabbitMQ connection attempt {Attempt}/{Max} failed: {Msg}", attempt, maxAttempts, ex.Message);
                if (attempt < maxAttempts) await Task.Delay(5000, ct);
            }
        }

        _logger.LogError("Could not connect to RabbitMQ after {Max} attempts", maxAttempts);
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}
