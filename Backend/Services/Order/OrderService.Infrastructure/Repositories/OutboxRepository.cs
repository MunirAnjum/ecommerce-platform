using Microsoft.EntityFrameworkCore;
using OrderService.Application.Interfaces;
using OrderService.Application.Outbox;
using OrderService.Infrastructure.Persistences;

namespace OrderService.Infrastructure.Repositories
{
    public class OutboxRepository : IOutboxRepository
    {
        private readonly OrderDbContext _context;

        public OutboxRepository(OrderDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(OutboxMessage message)
        {
            await _context.OutboxMessages.AddAsync(message);
        }

        public async Task<List<OutboxMessage>> GetPendingAsync(int batchSize = 20)
        {
            return await _context.OutboxMessages
                .Where(x => x.ProcessedOnUtc == null)
                .OrderBy(x => x.OccurredOnUtc)
                .Take(batchSize)
                .ToListAsync();
        }

        public async Task UpdateAsync(OutboxMessage message)
        {
            await _context.SaveChangesAsync();
        }
    }
}
