using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletPayment.Models.DataObjects;

namespace WalletPayment.Services.Interfaces
{
    public interface IEmail
    {
        Task<bool> SendEmail(EmailDto request,string emailUser);
        Task<bool> SendEmailPasswordReset(string Link, string emailUser);
        Task<bool> SendEmailVerifyUser(string Link, string emailUser);
        Task<bool> SendCreditEmail(string senderEmail, string recipient, string amount, string balance, string date, string username);
        Task<bool> SendDebitEmail(string recepientEmail, string sender, string amount2, string balance2, string date2, string username2);
        Task<bool> SendDepositEmail(string selfEmail, string selfName, string selfAmount, string selfBalance, string date3);
    }
}


