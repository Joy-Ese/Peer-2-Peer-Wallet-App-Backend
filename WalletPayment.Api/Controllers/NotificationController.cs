using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WalletPayment.Models.DataObjects;
using WalletPayment.Services.Interfaces;

namespace WalletPayment.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private INotification _notificationService;

        public NotificationController(INotification notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("GetAllNotifications"), Authorize]
        public async Task<IActionResult> GetAllNotifications()
        {
            var result = await _notificationService.GetAllNotifications();
            return Ok(result);
        }

        [HttpGet("GetAllUnreadNotifications"), Authorize]
        public async Task<IActionResult> GetAllUnreadNotifications()
        {
            var result = await _notificationService.GetAllUnreadNotifications();
            return Ok(result);
        }

        [HttpGet("GetAllUnreadNotificationsNo"), Authorize]
        public async Task<IActionResult> GetAllUnreadNotificationsNo()
        {
            var result = await _notificationService.GetAllUnreadNotificationsNo();
            return Ok(result);
        }

        [HttpPut("SetNotificationsToRead"), Authorize]
        public async Task<IActionResult> SetNotificationsToRead(GetUnreadNotificationDTO req)
        {
            var result = await _notificationService.SetNotificationsToRead(req);
            return Ok(result);
        }

    }
}
