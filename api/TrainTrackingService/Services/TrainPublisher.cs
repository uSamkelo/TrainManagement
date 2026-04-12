#nullable enable

using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using TrainTrackingService.Models;

public class TrainPublisher : ITrainPositionEventPublisher // Ensure the class name is NOT 'RabbitMQPublisher'
{
    // REMOVE 'readonly' from these two lines
    private IConnection? _connection;
    private IChannel? _channel;

    public async Task InitializeAsync(IConfiguration configuration)
    {
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
            Port = int.TryParse(configuration["RabbitMQ:Port"], out var port) ? port : 5672,
            UserName = configuration["RabbitMQ:UserName"] ?? "guest",
            Password = configuration["RabbitMQ:Password"] ?? "guest"
        };

        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        await _channel.ExchangeDeclareAsync(
                exchange: "TrainPositionEvents",
                type: ExchangeType.Direct,
                durable: true);
    }

    public async Task PublishTrainPosition(TrainPosition position)
    {
        if (_channel == null) throw new InvalidOperationException("Channel not initialized");

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(position));

        var properties = new BasicProperties();

        // Use the 5-argument overload required by v7
        await _channel.BasicPublishAsync(
            exchange: "TrainPositionEvents",
            routingKey: "train.position",
            mandatory: false,             // Required argument
            basicProperties: properties,
            body: body                    // This is now ReadOnlyMemory<byte>
        );
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel != null) await _channel.CloseAsync();
        if (_connection != null) await _connection.CloseAsync();
    }
}
