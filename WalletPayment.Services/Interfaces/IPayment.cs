using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletPayment.Models.DataObjects;

namespace WalletPayment.Services.Interfaces
{
    public interface IPayment
    {
        Task<PayStackResponseViewModel> InitializePaystackPayment(RequestDto req);
        Task<WebHookEventViewModel> WebHookPaystack(WebHookEventViewModel eventData);
    }
}



