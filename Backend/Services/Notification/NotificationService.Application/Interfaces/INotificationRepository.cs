using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationService.Application.Interfaces
{
    public interface INotificationRepository
    {
        Task AddAsync(Notification notification);

        Task<Notification?> GetByIdAsync(Guid id);

        Task<List<Notification>> GetByUserIdAsync(Guid userId);

        Task UpdateAsync(Notification notification);

        Task<Notification?> GetBySourceIdAsync(Guid sourceId, NotificationType type);
    }
}
