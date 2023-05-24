using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletPayment.Models.DataObjects;

namespace WalletPayment.Services.Interfaces
{
    public interface IDashboard
    {
        Task<UserDashboardViewModel> GetUserDetails();
        Task<UserBalanceViewModel> GetUserAccountBalance();
        Task<UserAcctNumViewModel> GetUserAccountNumber();
        Task<UserAcctCurrencyModel> GetUserAccountCurrency();
        Task<UserDashboardViewModel> GetUserEmail();
        Task<UserProfileViewModel> GetUserProfile();
        Task<UpdateUserInfoModel> UpdateUserInfo(UpdateUserInfoDto request);
        Task<ImageRequestViewModel> UploadNewImage(IFormFile fileData);
        Task<ImageRequestViewModel> UpdateImage(IFormFile fileData);
        Task<ImageViewModel> GetUserImage();
        Task<DeleteImageViewModel> DeleteUserImage();
        Task<SecurityQuestionViewModel> SetSecurityQuestion(SecurityQuestionDto request);
        Task<GetSecurityQuestionViewModel> GetUserSecurityQuestion();
        Task<bool> GetUserSecurityAnswer();
        Task<bool> GetUserPin();
        Task<bool> DoesUserHaveImage();
    }
}

