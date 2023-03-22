using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletPayment.Services.Interfaces
{
    public interface INotificationHub
    {
        Task SendNotificationToUser(string user, string message);
    }
}
