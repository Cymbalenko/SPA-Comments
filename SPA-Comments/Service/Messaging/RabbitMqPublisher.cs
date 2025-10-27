
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace Service.Messaging;

public class RabbitMqPublisher : IRabbitMqPublisher, IAsyncDisposable
{
    public const string RabbitCommentExcenge = "comments_exchange";
    public const string RabbitCommentQueue = "comments_queue";
    private readonly IConnection _connection;
    private readonly IChannel _channel;

    public RabbitMqPublisher(IConfiguration configuration)
    {
        var rabbitMqConfig = configuration.GetSection("RabbitMQ");
        var host = rabbitMqConfig["Host"];
        var port = rabbitMqConfig["Port"];
        var factory = new ConnectionFactory
        {
            HostName = host,
            Port = int.Parse(port)
        };
        Console.WriteLine($"Initializing RabbitMQ Host:{host}, Port:{port}");

        _connection = factory.CreateConnectionAsync().Result;
        _channel = _connection.CreateChannelAsync().Result;

    }

    public async Task PublishAsync<T>(string queue, string routingKey, T message)
    {
        // Объявляем обмен (если он еще не существует)
        await _channel.ExchangeDeclareAsync(
            exchange: RabbitCommentExcenge,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false
        );

        // Объявляем очередь (если она еще не существует)
        await _channel.QueueDeclareAsync(
            queue: queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        // Привязываем очередь к обмену с использованием ключа маршрутизации
        await _channel.QueueBindAsync(queue, RabbitCommentExcenge, routingKey);

        // Сериализуем сообщение
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        // Устанавливаем свойства для сообщения (например, чтобы оно не терялось)
        var props = new BasicProperties { DeliveryMode = DeliveryModes.Persistent };

        // Публикуем сообщение в обмен
        await _channel.BasicPublishAsync(RabbitCommentExcenge, routingKey, true, props, body);
    }

    public async ValueTask DisposeAsync()
    {
        await _channel.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
