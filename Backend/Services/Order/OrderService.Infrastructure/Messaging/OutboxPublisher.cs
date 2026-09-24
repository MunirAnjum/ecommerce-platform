using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderService.Application.Interfaces;

namespace OrderService.Infrastructure.Messaging
{
    public class OutboxPublisher : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public OutboxPublisher(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope =
                        _scopeFactory.CreateScope();

                    var repository =
                        scope.ServiceProvider
                            .GetRequiredService<IOutboxRepository>();

                    var publisher =
                        scope.ServiceProvider
                            .GetRequiredService<IRabbitMqPublisher>();

                    var messages =
                        await repository.GetPendingAsync();

                    foreach (var message in messages)
                    {
                        try
                        {
                            await publisher.PublishRawAsync(
                                message.RoutingKey,
                                message.Payload);

                            message.ProcessedOnUtc =
                                DateTime.UtcNow;

                            message.Error = null;

                            await repository.UpdateAsync(
                                message);
                        }
                        catch (Exception ex)
                        {
                            message.Error = ex.Message;

                            await repository.UpdateAsync(
                                message);

                            Console.WriteLine(
                                $"Outbox publish failed: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"Outbox worker error: {ex.Message}");
                }

                await Task.Delay(
                    TimeSpan.FromSeconds(5),
                    stoppingToken);
            }
        }
    }
}
