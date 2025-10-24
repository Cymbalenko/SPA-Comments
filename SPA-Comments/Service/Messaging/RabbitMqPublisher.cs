
using Microsoft.Extensions.Configuration;
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
    private readonly IConnection _connection;
    private readonly IChannel _channel;

    public RabbitMqPublisher(IConfiguration configuration)
    {
        var rabbitMqConfig = configuration.GetSection("RabbitMQ");
        var factory = new ConnectionFactory
        {
            HostName = rabbitMqConfig["Host"] 
        };

        _connection = factory.CreateConnectionAsync().Result;
        _channel = _connection.CreateChannelAsync().Result;

    }

    public async Task PublishAsync<T>(string exchange, string routingKey, T message)
    {
        // Объявляем обмен (если он еще не существует)
        await _channel.ExchangeDeclareAsync(exchange, ExchangeType.Topic, durable: true);

        // Объявляем очередь (если она еще не существует)
        await _channel.QueueDeclareAsync(exchange, durable: true, exclusive: false, autoDelete: false, arguments: null);

        // Привязываем очередь к обмену с использованием ключа маршрутизации
        await _channel.QueueBindAsync(exchange, exchange, routingKey);

        // Сериализуем сообщение
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        // Устанавливаем свойства для сообщения (например, чтобы оно не терялось)
        var props = new BasicProperties { DeliveryMode = DeliveryModes.Persistent };

        // Публикуем сообщение в обмен
        await _channel.BasicPublishAsync(exchange, routingKey, true, props, body);
    }

    public async ValueTask DisposeAsync()
    {
        await _channel.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
