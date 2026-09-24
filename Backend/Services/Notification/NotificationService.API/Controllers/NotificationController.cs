using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.DTOs;
using System.IdentityModel.Tokens.Jwt;
using NotificationService.Application.Interfaces;
using System.Security.Claims;

namespace NotificationService.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _noficationService;

        public NotificationController(INotificationService notificationService)
        {
            _noficationService = notificationService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateNotificationRequest request)
        {
            var notification = await _noficationService.CreateAsync(request);

            return Ok(notification);
        }

        [HttpGet("{notificationId:guid}")]
        public async Task<IActionResult> GetById(Guid notificationId)
        {
            var userId = GetUserId();

            var notification = await _noficationService.GetByIdAsync(userId, notificationId);

            return Ok(notification);
        }

        [HttpGet]
        public async Task<IActionResult> GetMyNotifications()
        {
            var userId = GetUserId();

            var notification = await _noficationService.GetMyNotificationsAsync(userId);

            return Ok(notification);
        }

        [HttpPost("{notificationId:guid}/send")]
        public async Task<IActionResult> Send(Guid notificationId)
        {
            var userId = GetUserId();

            var notification = await _noficationService.SendAsync(userId, notificationId);

            return Ok(notification);
        }
        private Guid GetUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if(!Guid.TryParse(value, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid user identity.");
            }

            return userId;
        }
    }
}
