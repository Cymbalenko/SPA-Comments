using Dal.Models;
using Dto.Comment;
using Microsoft.EntityFrameworkCore.Metadata;
using Nest;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Service.Messaging;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPA_Comments.Server.Hubs
{
    public class RabbitMqElasticsearchListener : BackgroundService
    {
        private readonly IElasticClient _elasticClient;
        private IConnection _connection;
        private IChannel _channel; 

        private readonly IConfiguration _configuration;

        public RabbitMqElasticsearchListener(IElasticClient elasticClient, IConfiguration configuration)
        {
            _configuration = configuration;
            _elasticClient = elasticClient;
        }

        // Асинхронная инициализация подключения и канала
        private async Task InitializeRabbitMqAsync()
        {
            // Подключение к RabbitMQ
            var rabbitMqConfig = _configuration.GetSection("RabbitMQ");

            var factory = new ConnectionFactory
            {
                HostName = rabbitMqConfig["Host"]
            };

            _connection = await factory.CreateConnectionAsync();  // Асинхронное создание соединения
            _channel = await _connection.CreateChannelAsync();  // Асинхронное создание канала

            // Декларируем очередь
            await _channel.QueueDeclareAsync(RabbitMqPublisher.RabbitCommentExcenge, durable: true, exclusive: false, autoDelete: false, arguments: null);
            await _channel.BasicQosAsync(0, 1, false); // Параметры очереди
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Инициализация RabbitMQ при запуске фонового сервиса
            await InitializeRabbitMqAsync();

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var comment = JsonSerializer.Deserialize<CommentDto>(message);

                // Добавляем комментарий в Elasticsearch
                var indexResponse = await _elasticClient.IndexDocumentAsync(comment);

                if (!indexResponse.IsValid)
                {
                    // Логируем ошибку или добавляем обработку ошибок
                    Console.WriteLine($"Error indexing comment with ID: {comment.Id}");
                }

                // Подтверждаем получение сообщения
                await _channel.BasicAckAsync(ea.DeliveryTag, false);
            };

            // Начинаем слушать очередь
            await _channel.BasicConsumeAsync(RabbitMqPublisher.RabbitCommentExcenge, autoAck: false, consumer);

            // Ожидание, пока не будет отменено
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken); // Даем время на обработку
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            // Закрытие канала и соединения при завершении работы фонового сервиса
            await _channel.CloseAsync();
            await _connection.CloseAsync();
            await base.StopAsync(cancellationToken);
        }
    }
}
