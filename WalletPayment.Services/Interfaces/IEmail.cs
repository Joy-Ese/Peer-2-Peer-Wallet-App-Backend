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
        Task<ForgetPasswordModel> ForgetPassword(ForgetPasswordDto emailReq);
        Task<GetResetPasswordModel> GetResetPassword(string token, string email);
        Task<ResetPasswordModel> ResetPassword(ResetPasswordDto resetPasswordReq);
        Task<bool> SendCreditEmail(string senderEmail);
        Task<bool> SendDebitEmail(string recepientEmail);
    }
}


