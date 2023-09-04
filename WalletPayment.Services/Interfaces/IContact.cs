using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletPayment.Models.DataObjects;

namespace WalletPayment.Services.Interfaces
{
    public interface IContact
    {
        Task<RespnModel<SearchContactModel>> GetSearchedContact(string searchInfo);
        Task<ChatTrnModel> GetChatTrnAcctNums(string currency, string destAcctUser);
    }
}
