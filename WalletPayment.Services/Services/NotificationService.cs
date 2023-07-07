using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WalletPayment.Models.Entites;
using WalletPayment.Services.Data;
using WalletPayment.Services.Interfaces;

namespace WalletPayment.Services.Services
{
    public class NotificationService : INotification
    {
        private readonly IHubContext<NotificationSignalR> _hub;
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(DataContext context, IHttpContextAccessor httpContextAccessor, ILogger<NotificationService> logger, IHubContext<NotificationSignalR> hub)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _logger.LogDebug(1, "Nlog injected into DashboardService");
            _hub = hub;
        }

        public async Task SendNotification(string user, string message)
        {
            try
            {
                await _hub.Clients.All.SendAsync("RecieveTransferAlert", user, message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return;
            }
        }

        public async Task SendTransferNotification(int recieverId, string currency, string amount, string sender)
        {
            try
            {
                Notification newNotification = new Notification
                {
                    Title = $"You just got an alert from {sender}",
                    Description = $"You just got a credit alert of {currency}{amount} from {sender}",
                    Date = DateTime.Now,
                    NotificationUserId = recieverId,
                    IsNotificationRead = false,
                };

                await _context.Notifications.AddAsync(newNotification);
                await _context.SaveChangesAsync();

                var user = await _context.Users.Where(x => x.Id == recieverId).FirstOrDefaultAsync();

                if (user == null) return;

                await SendNotification(user.Username, newNotification.Title);
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return;
            }
        }

    }
}
