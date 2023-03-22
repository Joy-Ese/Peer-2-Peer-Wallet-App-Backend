using Microsoft.AspNetCore.SignalR;
using WalletPayment.Services.Interfaces;

namespace WalletPayment.Api.Hubs
{
    public class NotificationHub : Hub<INotificationHub>
    {
        public async Task SendNotificationToUser(string user, string message)
        {
            if (string.IsNullOrEmpty(user))
            {
                await Clients.All.SendNotificationToUser("receivedNotification", message);
            }
            await Clients.User(user).SendNotificationToUser("receivedNotification", message);
        }
    }
}
