using Microsoft.AspNetCore.SignalR;

namespace WalletPayment.Services.Services
{
    public class NotificationSignalR : Hub
    {
        public static int totalNotifi { get; set; } = 0;

        public async Task RegisterNotification(string from, string to, string message)
        {
            totalNotifi++;
            await Clients.All.SendAsync("GetNotification", from, to, message);
        }

        public async Task SendTransferAlert(string user, string message)
        {
            await Clients.All.SendAsync("RecieveTransferAlert", user, message);
        }
    }
}
