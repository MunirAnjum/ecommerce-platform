using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using NotificationService.Application.Events;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using NotificationService.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NotificationService.Infrastructure.Configuration;

namespace NotificationService.Infrastructure.Messaging;

public class RabbitMqConsumer : BackgroundService
{
    private const string QueueName = "payment-completed";
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqSettings _settings;
    public RabbitMqConsumer(IServiceScopeFactory scopeFactory, IOptions<RabbitMqSettings> options)
    {
        _scopeFactory = scopeFactory;
        _settings = options.Value;
    }
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
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
            routingKey: RabbitMqConstants.PaymentCompletedRoutingKey);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (sender, args) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(args.Body.ToArray());

                var paymentEvent =
                    JsonSerializer.Deserialize<PaymentCompletedEvent>(json);

                if(paymentEvent is null)
                {
                    Console.WriteLine("Failed to deserialize the paymentCompletedEvent.");

                    return;
                }

                using var scope = _scopeFactory.CreateScope();

                var notificationService =
                    scope.ServiceProvider
                        .GetRequiredService<INotificationService>();

                await notificationService.ProcessPaymentCompletedAsync(
                    paymentEvent);

                await channel.BasicAckAsync(deliveryTag: args.DeliveryTag,
                    multiple: false);

                Console.WriteLine("PaymentCompletedEvent processed successfully.");

            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error processing RabbitMQ message: {ex.Message}");

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