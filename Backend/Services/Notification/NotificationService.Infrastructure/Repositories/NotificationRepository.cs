using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Infrastructure.Persistences;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationService.Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly NotificationDbContext _context;

        public NotificationRepository(NotificationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);

            await _context.SaveChangesAsync();
        }

        public async Task<Notification?> GetByIdAsync(Guid id)
        {
            return await _context.Notifications.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Notification>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Notifications.Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt).ToListAsync();
        }

        public async Task UpdateAsync(Notification notification)
        {
            await _context.SaveChangesAsync();
        }

        public async Task<Notification?> GetBySourceIdAsync(Guid sourceId, NotificationType type)
        {
            return await _context.Notifications.FirstOrDefaultAsync(x => x.SourceId == sourceId && x.Type == type);
        } 
    }
}
