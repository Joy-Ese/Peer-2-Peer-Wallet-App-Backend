//using Microsoft.AspNetCore.SignalR;
//using System.Timers;
//using WalletPayment.Services.Services;

//namespace Worker
//{
//    //public class Worker : BackgroundService
//    //{
//    //    private readonly ILogger<Worker> _logger;

//    //    public Worker(ILogger<Worker> logger)
//    //    {
//    //        _logger = logger;
//    //    }

//    //    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//    //    {
//    //        while (!stoppingToken.IsCancellationRequested)
//    //        {
//    //            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
//    //            await Task.Delay(1000, stoppingToken);
//    //        }
//    //    }
//    //}

//    public class NotificationService : IHostedService
//    {
//        private Task _executingTask;
//        private CancellationTokenSource _cts;
//        private readonly IHubContext<NotificationSignalR> _notificationHubContext;
//        private readonly ILogger<NotificationService> _logger;
//        private readonly System.Timers.Timer _timer;

//        public NotificationService(IHubContext<NotificationSignalR> notificationHubContext, ILogger<NotificationService> logger)
//        {
//            _notificationHubContext = notificationHubContext;
//            _logger = logger;
//            _timer = new System.Timers.Timer(5);
//            _timer.Elapsed += GenerateAndSendFeed;
//        }

//        private void GenerateAndSendFeed(object? sender, ElapsedEventArgs e)
//        {
//            var notification = CreateNotificationEntity(Guid.NewGuid().ToString());
//            _notificationHubContext.Clients.All.SendAsync("GetNotification", notification);
//            //_notificationHubContext.Clients.Client(notification.Id).SendAsync("GetNotificationBYID", notification);
//        }

//        private WalletPayment.Models.DataObjects.Notification CreateNotificationEntity(string id)
//        {
//            return new WalletPayment.Models.DataObjects.Notification
//            {
//                Id = id,
//                NotificationInfo = $"You just got credited by ... Have a great day!",
//                CreatedAt = DateTime.Now,
//            };
//        }

//        public Task StartAsync(CancellationToken cancellationToken)
//        {
//            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

//            _timer.Start();
//            return _executingTask.IsCompleted ? _executingTask : Task.CompletedTask;
//        }

//        public Task? StopAsync(CancellationToken cancellationToken)
//        {
//            if (_executingTask == null)
//            {
//                return null;
//            }
//            _cts.Cancel();

//            _timer.Enabled = false;
//            _timer.Dispose();
//            return Task.CompletedTask;
//        }
//    }
//}