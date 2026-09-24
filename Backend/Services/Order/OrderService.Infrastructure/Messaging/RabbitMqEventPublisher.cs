using Microsoft.Extensions.Options;
using OrderService.Application.Events;
using OrderService.Application.Interfaces;
using OrderService.Infrastructure.Configuration;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace OrderService.Infrastructure.Messaging
{
    public class RabbitMqEventPublisher : IRabbitMqPublisher
    {
        private readonly RabbitMqSettings _settings;

        public RabbitMqEventPublisher(IOptions<RabbitMqSettings> options)
        {
            _settings = options.Value;
        }

        public async Task PublishRawAsync(string routingKey, string payload)
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                UserName = _settings.UserName,
                Password = _settings.Password
            };

            await using var connection = await factory.CreateConnectionAsync();

            await using var channel = await connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(
                exchange: RabbitMqConstants.EventExchange,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false);

            var body = Encoding.UTF8.GetBytes(payload);

            await channel.BasicPublishAsync(
                exchange: RabbitMqConstants.EventExchange,
                routingKey: routingKey,
                body: body);
        }
    }
}
