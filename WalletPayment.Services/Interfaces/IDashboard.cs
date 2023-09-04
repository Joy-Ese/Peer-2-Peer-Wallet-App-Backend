using Microsoft.AspNetCore.Http;
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
        Task<KycViewModel> KycUpload(IFormFile fileData, string fileCode);
        Task<List<KycAdminViewModel2>> GetUserInfoOnKycUploadsForAdmin();
        Task<KycViewModel> RemoveImage(KycRejectDTO req, string filename, string userId, string filecode);
        Task<KycViewModel> AcceptImage(string filename, string userId, string filecode);
        Task<UserProfileModel> GetUserProfileLevel();
        Task<List<AllAdminsListModel>> AllAdminsLists();
        Task<ResponseModel> DisableEnableAdmin(DisableEnableAdminDTO req);
        Task<bool> AdminLogout(string adminUsername);
        Task<List<KycDocs>> GetUnavailableDocuments();
        Task<List<UsersForAdmin>> GetUsersInSysAdmin();
    }
}

