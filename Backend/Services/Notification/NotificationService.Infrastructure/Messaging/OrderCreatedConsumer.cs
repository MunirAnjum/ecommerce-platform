using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NotificationService.Application.Events;
using NotificationService.Application.Interfaces;
using NotificationService.Infrastructure.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace NotificationService.Infrastructure.Messaging
{
    public class OrderCreatedConsumer : BackgroundService
    {
        private const string QueueName = "order-created";

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly RabbitMqSettings _settings;

        public OrderCreatedConsumer(IServiceScopeFactory scopeFactory, IOptions<RabbitMqSettings> options)
        {
            _scopeFactory = scopeFactory;
            _settings = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                UserName = _settings.UserName,
                Password = _settings.Password
            };

            await using var connection =
            await factory.CreateConnectionAsync(stoppingToken);

            await using var channel =
                await connection.CreateChannelAsync(
                    cancellationToken: stoppingToken);

            await channel.ExchangeDeclareAsync(
                exchange: RabbitMqConstants.EventExchange,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false);

            await channel.QueueDeclareAsync(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: stoppingToken);

            await channel.QueueBindAsync(
                queue: QueueName,
                exchange: RabbitMqConstants.EventExchange,
                routingKey: RabbitMqConstants.OrderCreatedRoutingKey);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (sender, args) =>
            {
                try
                {
                    var json =
                    Encoding.UTF8.GetString(args.Body.ToArray());

                    var orderEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(json);

                    if(orderEvent is null)
                    {
                        Console.WriteLine("Failed to deserialized OrderCreatedEvent.");

                        await channel.BasicNackAsync(
                            args.DeliveryTag,
                            false,
                            false);

                        return;
                    }

                    using var scope = _scopeFactory.CreateScope();

                    var notificationService =
                    scope.ServiceProvider
                        .GetRequiredService<INotificationService>();

                    await notificationService
                        .ProcessOrderCreatedAsync(orderEvent);

                    await channel.BasicAckAsync(
                        deliveryTag: args.DeliveryTag,
                        multiple: false);

                    Console.WriteLine(
                        "OrderCreatedEvent processed successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing OrderCreatedEvent: {ex.Message}");

                    await channel.BasicNackAsync(
                        deliveryTag: args.DeliveryTag,
                        multiple: false,
                        requeue: true);
                }
            };

            await channel.BasicConsumeAsync(
                    queue: QueueName,
                    autoAck: false,
                    consumer: consumer,
                    cancellationToken: stoppingToken);

            await Task.Delay(
                Timeout.Infinite,
                stoppingToken);
        }
    }
}
