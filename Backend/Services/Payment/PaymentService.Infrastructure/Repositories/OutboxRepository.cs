using Microsoft.EntityFrameworkCore;
using PaymentService.Application.Outbox;
using PaymentService.Infrastructure.Persistences;
using PaymentService.Application.Interfaces;

namespace PaymentService.Infrastructure.Repositories
{
    public class OutboxRepository : IOutboxRepository
    {
        private readonly PaymentDbContext _context;

        public OutboxRepository(PaymentDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(OutboxMessage message)
        {
            await _context.OutboxMessages.AddAsync(message);
        }

        public async Task<List<OutboxMessage>> GetPendingAsync(
            int batchSize = 20)
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
