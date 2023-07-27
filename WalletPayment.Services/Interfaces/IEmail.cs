using Microsoft.AspNetCore.Http;
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
        Task<bool> SendEmail(string subject, string userEmail, string body);
        Task<bool> SendEmailPasswordReset(string Link, string emailUser);
        Task<bool> SendEmailVerifyUser(string Link, string emailUser, string subject);
        Task<bool> SendCreditEmail(string senderEmail, string recipient, string amount, string balance, string date, string username, string currency, string acctNum);
        Task<bool> SendDebitEmail(string recepientEmail, string sender, string amount2, string balance2, string date2, string username2, string currency2, string acctNum2);
        Task<bool> SendDepositEmail(string selfEmail, string selfName, string selfAmount, string selfBalance, string date3, string currency, string acctNum);
        Task<bool> SendStatementAsAttachment(string userName, string userEmail, string currencyEmail, IFormFile attachments, string fileType);
        Task<bool> SendCreateWalletEmail(string recepientEmail, string amount, string currency, string firstName, string date, string balance);
    }
}


