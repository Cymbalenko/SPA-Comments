using Dal.Models;
using Dto.Comment;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
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
        private readonly IHubContext<CommentHub> _hubContext;
        private readonly IElasticClient _elasticClient;
        private IConnection _connection;
        private IChannel _channel; 

        private readonly IConfiguration _configuration;

        public RabbitMqElasticsearchListener(IElasticClient elasticClient, IConfiguration configuration,
                                         IHubContext<CommentHub> hubContext)
        {
            _configuration = configuration;
            _elasticClient = elasticClient;
            _hubContext = hubContext;
        }


        private async Task InitializeRabbitMqAsync()
        {
            Console.WriteLine("Initializing RabbitMQ connection...");
            var rabbitMqConfig = _configuration.GetSection("RabbitMQ");
            string host = rabbitMqConfig["Host"];
            string port = rabbitMqConfig["Port"];
            var factory = new ConnectionFactory
            {
                HostName = host,
                Port = int.Parse(port)
            };
            Console.WriteLine($"Initializing RabbitMQ Host:{host}, Port:{port}");

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
            Console.WriteLine($"Initializing RabbitMQ connection");

            await _channel.QueueDeclareAsync(RabbitMqPublisher.RabbitCommentQueue, durable: true, exclusive: false, autoDelete: false, arguments: null);
            await _channel.BasicQosAsync(0, 1, false);

            Console.WriteLine($"Initializing RabbitMQ connected");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {

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
                    await _hubContext.Clients.All.SendAsync("ReceiveComment", comment);
                    // Подтверждаем получение сообщения
                    await _channel.BasicAckAsync(ea.DeliveryTag, false);
                };

                // Начинаем слушать очередь
                await _channel.BasicConsumeAsync(RabbitMqPublisher.RabbitCommentQueue, autoAck: false, consumer);

                // Ожидание, пока не будет отменено
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken); // Даем время на обработку
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                // Закрытие канала и соединения при завершении работы фонового сервиса
                if (_channel != null)
                    await _channel.CloseAsync();
                if (_connection != null)
                    await _connection.CloseAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            } 
        }
    }
}
