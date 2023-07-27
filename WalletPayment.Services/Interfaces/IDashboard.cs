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
        Task<bool> NoSecurityAttemptsLeft();
        Task<KycViewModel> KycUpload(IFormFile fileData);
        Task<KycViewModel> KycReUpload(IFormFile fileData);
        Task<List<UserInfoOnKycUploadsForAdminModel>> GetUserInfoOnKycUploadsForAdmin();
        Task<List<KycAdminViewModel>> GetKycUploadsForAdmin();
        Task<KycViewModel> RemoveImage(string filename, string userId);
        Task<KycViewModel> AcceptImage(string filename, string userId);
        Task<UserProfileModel> GetUserProfileLevel();
        Task<List<AllAdminsListModel>> AllAdminsLists();
        Task<ResponseModel> DisableEnableAdmin(DisableEnableAdminDTO req);
        Task<bool> GetKycStatus();
    }
}

