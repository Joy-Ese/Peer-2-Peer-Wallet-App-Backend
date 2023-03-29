using Microsoft.AspNetCore.SignalR;

namespace WalletPayment.Api.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly DataContext _context;

        public NotificationHub(DataContext context)
        {
            _context = context;
        }

        public async Task SendNotification(string user, string message)
        {
            if (string.IsNullOrEmpty(user))
            {
                await Clients.All.SendAsync("ReceiveNotification", user, message);
            }
            await Clients.User(user).SendAsync("ReceiveNotification", user, message);
        }
    }
}
