using PaymentService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Infrastructure.Persistences
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly PaymentDbContext _context;

        public UnitOfWork(PaymentDbContext context)
        {
            _context = context;
        }

        public async Task ExecuteInTransactionAsync(
            Func<Task> action)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                await action();

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
