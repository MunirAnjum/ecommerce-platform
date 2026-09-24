using OrderService.Application.Outbox;

namespace OrderService.Application.Interfaces
{
    public interface IOutboxRepository
    {
        Task AddAsync(OutboxMessage message);

        Task<List<OutboxMessage>> GetPendingAsync(int batchSize = 20);

        Task UpdateAsync(OutboxMessage message);
    }
}
