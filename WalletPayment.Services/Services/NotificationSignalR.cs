using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WalletPayment.Models.DataObjects;
using WalletPayment.Models.Entites;
using WalletPayment.Services.Data;
using WalletPayment.Services.Interfaces;

namespace WalletPayment.Services.Services
{
    public class NotificationSignalR : Hub
    {
        private readonly DataContext _context;

        public NotificationSignalR(DataContext context)
        {
            _context = context;
        }


        public async Task RegisterNotification(string from, string to, string message)
        {
            await Clients.All.SendAsync("GetNotification", from, to, message);
        }


        public async Task SendTransferAlert(string user, string message) 
        {
            await Clients.All.SendAsync("RecieveTransferAlert", user, message);
        }


        public async Task OnLogOut(string username)
        {
            var setUserToFalse = await _context.Users.Where(x => x.Username == username).FirstOrDefaultAsync();

            setUserToFalse.IsUserLogin = false;
            _context.SaveChanges();

            Clients.All.SendAsync("UpdateAdmin");
        }


        public async Task onAdminLogOut(string username)
        {
            var setAdminToFalse = await _context.Adminss.Where(x => x.Username == username).FirstOrDefaultAsync();

            setAdminToFalse.IsUserLogin = false;
            _context.SaveChanges();

            Clients.All.SendAsync("UpdateUser");
        }


        public async Task SendMessage(string userName, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", userName, message);
        }


        public async Task User2UserSendMessage(string userName, string message)
        {
            await Clients.All.SendAsync("User2UserReceiveMessage", userName, message);
        }


    }
}
