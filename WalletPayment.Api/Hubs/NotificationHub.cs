using Microsoft.AspNetCore.SignalR;
using WalletPayment.Models.DataObjects;

namespace WalletPayment.Api.Hubs
{
    public class NotificationHub : Hub
    {
        //private readonly DataContext _context;

        public NotificationHub()
        {
        }

        public async Task RegisterNotification(string user, string message)
        {
            //var users = await _context.Users.FirstOrDefaultAsync();

            await Clients.All.SendAsync("GetNotification", user, message);

            //if (true)
            //{
            //    await Clients.All.SendAsync("GetNotification", user, message);
            //}
            //await Clients.User(user).SendAsync("GetNotification", user, message);
        }
    }
}
