using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WalletPayment.Models.DataObjects;
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
            _logger.LogDebug(1, "Nlog injected into NotificationService");
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

        public async Task<List<GetNotificationModel>> GetAllNotifications()
        {
            List<GetNotificationModel> getNotification = new List<GetNotificationModel>();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return getNotification;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var allUserNotifications = await _context.Notifications.Include("NotificationUser").Where(x => x.NotificationUserId == userID)
                            .Select(x => new GetNotificationModel
                            {
                                id =  x.Id,
                                username = x.NotificationUser.Username,
                                heading = x.Title,
                                summary = $"Hi {x.NotificationUser.Username}, {x.Description}",
                                date = DateTime.Now,
                                isRead = x.IsNotificationRead,

                            }).ToListAsync();

                if (allUserNotifications == null) return getNotification;

                return allUserNotifications;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return getNotification;
            }
        }

        public async Task<List<GetUnreadNotificationModel>> GetAllUnreadNotifications()
        {
            List<GetUnreadNotificationModel> getUnreadNotification = new List<GetUnreadNotificationModel>();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return getUnreadNotification;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var allUserUnreadNotifications = await _context.Notifications
                            .Where(x => x.NotificationUserId == userID && x.IsNotificationRead == false)
                            .Select(x => new GetUnreadNotificationModel
                            {
                                id = x.Id,
                                heading = x.Title,
                            }).ToListAsync();

                if (allUserUnreadNotifications == null) return getUnreadNotification;

                return allUserUnreadNotifications;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return getUnreadNotification;
            }
        }

        public async Task<GetAllUnreadNotificationsNoModel> GetAllUnreadNotificationsNo()
        {
            GetAllUnreadNotificationsNoModel getUnreadNotificationNo = new GetAllUnreadNotificationsNoModel();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return getUnreadNotificationNo;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var allUserUnreadNotifications = await _context.Notifications
                            .Where(x => x.NotificationUserId == userID && x.IsNotificationRead == false)
                            .CountAsync();

                if (allUserUnreadNotifications == null) return getUnreadNotificationNo;

                getUnreadNotificationNo.allNotifications = allUserUnreadNotifications;
                return getUnreadNotificationNo;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return getUnreadNotificationNo;
            }
        }

        public async Task<bool> SetNotificationsToRead(GetUnreadNotificationDTO req)
        {
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return false;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var username = await _context.Users.Where(x => x.Id == userID).FirstOrDefaultAsync();

                var allUserUnreadNotifications = await _context.Notifications
                            .Where(x => x.NotificationUserId == userID && x.Id == req.id).FirstOrDefaultAsync();

                if (allUserUnreadNotifications.IsNotificationRead == true)
                {
                    return true;
                }

                allUserUnreadNotifications.IsNotificationRead = true;
                await _context.SaveChangesAsync();

                if (allUserUnreadNotifications == null) return false;

                await _hub.Clients.All.SendAsync("UpdateNotification", username.Username);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return false;
            }
        }



    }
}
