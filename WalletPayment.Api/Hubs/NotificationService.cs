//using Microsoft.AspNetCore.SignalR;
//using System.Timers;
//using WalletPayment.Api.Hubs;
//using WalletPayment.Models.DataObjects;

//namespace WalletPayment.Services.Services
//{
//    public class NotificationService : IHostedService
//    {
//        private readonly DataContext _context;
//        private readonly IHubContext<NotificationHub> _notificationHubContext;
//        private readonly ILogger<NotificationService> _logger;
//        private readonly System.Timers.Timer _timer;

//        public NotificationService(DataContext context, IHubContext<NotificationHub> notificationHubContext, ILogger<NotificationService> logger)
//        {
//            _context = context;
//            _notificationHubContext = notificationHubContext;
//            _logger = logger;
//            _timer = new System.Timers.Timer(5);
//            _timer.Elapsed += GenerateAndSendFeed;
//        }

//        private void GenerateAndSendFeed(object? sender, ElapsedEventArgs e)
//        {
//            var notification = CreateNotificationEntity(Guid.NewGuid().ToString());
//            _notificationHubContext.Clients.All.SendAsync("GetNotification", notification);
//            _notificationHubContext.Clients.Client(notification.Id).SendAsync("GetNotificationBYID", notification);
//        }

//        private Notification CreateNotificationEntity(string id)
//        {
//            return new Notification
//            {
//                Id = id,
//                NotificationInfo = $"You just got credited by ... Have a great day!",
//                CreatedAt = DateTime.Now,
//            };
//        }

//        public Task StartAsync(CancellationToken cancellationToken)
//        {
//            _timer.Start();
//            return Task.CompletedTask;
//        }

//        public Task StopAsync(CancellationToken cancellationToken)
//        {
//            _timer.Enabled = false;
//            _timer.Dispose();
//            return Task.CompletedTask;
//        }
//    }
//}
