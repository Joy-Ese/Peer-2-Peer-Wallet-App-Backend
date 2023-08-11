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


        //public async Task AuthLoginUser (SignalrUserModel personUser)
        //{
        //    string currentSignalrID = Context.ConnectionId;
        //    var user = _context.Users.Where(p => p.Username == personUser.username).SingleOrDefault();

        //    ReturnedModel newUser = new ReturnedModel
        //    {
        //        id = user.Id.ToString(),
        //        username = user.Username,
        //        email = user.Email,
        //        connId = currentSignalrID
        //    };

        //    if (user != null && AuthService.VerifyPasswordHash(personUser.password, user.PasswordHash, user.PasswordSalt))
        //    {
        //        SignalrConnection sigConnection = new SignalrConnection
        //        {
        //            UserId = user.Id,
        //            SignalrId = currentSignalrID,
        //            TimeStamp = DateTime.Now
        //        };
        //        await _context.SignalrConnections.AddAsync(sigConnection);
        //        await _context.SaveChangesAsync();


        //        await Clients.Caller.SendAsync("AuthLoginSuccess", newUser);
        //        await Clients.Others.SendAsync("UserOn", newUser);
        //    }

        //    await Clients.Caller.SendAsync("AuthLoginFail");
        //}


        public async Task OnLogOut(string username)
        {
            var setUserToFalse = await _context.Users.Where(x => x.Username == username).FirstOrDefaultAsync();

            setUserToFalse.IsUserLogin = false;
            _context.SaveChanges();

            Clients.All.SendAsync("UpdateAdmin");
        }


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// CHAT SIGNAL R
        //public async Task SendMessage(string user, string message, string role)
        //{
        //    string sendNameofChatter = string.Empty;
        //    if (role.Contains("Admin"))
        //    {
        //       save chat in DB in adminUserId
        //    }

        //    sendNameofChatter = await _context.Users.Where(u => u.Username == user).Select(x => x.Username).FirstOrDefaultAsync();
        //    if (sendNameofChatter == null)
        //    {
        //        sendNameofChatter = "User";
        //    }

        //    await Clients.All.SendAsync("ReceiveMessage", user, message, sendNameofChatter);
        //}


        public async Task SendMessage(string userName, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", userName, message);
        }


    }
}
