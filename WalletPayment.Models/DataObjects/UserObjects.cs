using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WalletPayment.Models.Entites;

namespace WalletPayment.Models.DataObjects
{
    public class ResponseModel
    {
        public bool status { get; set; }
        public string message { get; set; } = string.Empty;
    }

    public class ImageRequestViewModel
    {
        public bool status { get; set; }
        public string message { get; set; } = string.Empty;
    }

    public class ImageRequestDTO
    {
        public IFormFile ImageDetails { get; set; }
    }

    public class ImageViewModel
    {
        public byte[] imageDetails { get; set; }
    }

    public class DeleteImageViewModel
    {
        public bool status { get; set; }
        public string message { get; set; } = string.Empty;
    }

    public class SecurityQuestionDto
    {
        public string question { get; set; } = string.Empty;
        public string answer { get; set; } = string.Empty;
    }

    public class GetSecurityQuestionViewModel
    {
        public string Question { get; set; } = string.Empty;
    }

    public class SecurityQuestionViewModel
    {
        public bool status { get; set; }
        public string message { get; set; } = string.Empty;
    }

    public class UserDashboardViewModel
    {
        public string Username { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public List<AccountDetails> AccountDetails { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }

    public class TransactionListModel
    {
        public decimal amount { get; set; }
        public string senderInfo { get; set; } = string.Empty;
        public string recepientInfo { get; set; } = string.Empty;
        public string transactionType { get; set; } = string.Empty;
        public string currency { get; set; } = string.Empty;
        public DateTime date { get; set; }
        public string status { get; set; } = string.Empty;
    }

    public class TransactionResponseModel
    {
        public bool status { get; set; }
        public string responseMessage { get; set; } = string.Empty;
    }

    public class UserSignUpDto
    {
        public string username { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public string confirmPassword { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string phoneNumber { get; set; } = string.Empty;
        public string firstName { get; set; } = string.Empty;
        public string lastName { get; set; } = string.Empty;
        public string address { get; set; } = string.Empty;
    }

    public class UserLoginDto
    {
        [Required]
        public string username { get; set; } = string.Empty;
        [Required]
        public string password { get; set; } = string.Empty;
    }

    public class AccountViewModel
    {
        public string firstName { get; set; } = string.Empty;
        public string lastName { get; set; } = string.Empty;
        public string acctNum { get; set; } = string.Empty;
        //public List<AccountDetails> accountDetails { get; set; }
        public bool status { get; set; }
    }

    public class TransactionDto
    {
        public string sourceAccount { get; set; } = string.Empty;
        public string destinationAccount { get; set; } = string.Empty;
        public decimal amount { get; set; }
        public string pin { get; set; } = string.Empty;
    }

    public class TransactionDateDto
    {
        public string startDate { get; set; } = string.Empty;
        public string endDate { get; set; } =  string.Empty;
    }

    public class LoginViewModel
    {
        public bool status { get; set; }
        public string result { get; set; } = string.Empty;
        public string refreshedToken { get; set; } = string.Empty;
    }

    public class RegisterViewModel
    {
        public bool status { get; set; }
        public string message { get; set; } = string.Empty;
    }

    public class RefreshTokenViewModel
    {
        public string Token { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime ExpiresAt { get; set; }
    }

    public class LoginRefreshModel
    {
        public bool status { get; set; }
        public string message { get; set; } = string.Empty;
        public string token { get; set; } = string.Empty;
        public string refreshToken { get; set; } = string.Empty;
    }

    public class UserBalanceViewModel
    {
        public List<AccountDetails> AccountDetails { get; set; }
    }

    public class UserAcctNumViewModel
    {
        public List<AccountDetails> AccountDetails { get; set; }
    }

    public class UserAcctCurrencyModel
    {
        public List<AccountDetails> AccountDetails { get; set; }
    }

    public class ForgetPasswordDto
    {
        public string email { get; set; } = string.Empty;
    }

    public class ForgetPasswordModel
    {
        public bool status { get; set; }
        public string message { get; set; } = string.Empty;
    }

    public class ResetPasswordDto
    {
        public string email { get; set; } = string.Empty;
        public string token { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public string conPassword { get; set; } = string.Empty;
    }

    public class ResetPasswordModel
    {
        public bool status { get; set; }
        public string message { get; set; } = string.Empty;
    }

    public class VerifyEmailDto
    {
        public string email { get; set; } = string.Empty;
        public string token { get; set; } = string.Empty;
    }

    public class VerifyEmailModel
    {
        public bool status { get; set; }
        public string message { get; set; } = string.Empty;
    }

    public class ChangePasswordDto
    {
        public string answer { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public string confirmPassword { get; set; } = string.Empty;
    }

    public class ChangePasswordModel
    {
        public bool status { get; set; }
        public bool isLocked { get; set; }
        public string message { get; set; } = string.Empty;
    }

    public class UpdateUserInfoDto
    {
        public string firstName { get; set; } = string.Empty;
        public string lastName { get; set; } = string.Empty;
        public string username { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string phoneNumber { get; set; } = string.Empty;
        public string address { get; set; } = string.Empty;
    }

    public class UpdateUserInfoModel
    {
        public bool status { get; set; }
        public string message { get; set; } = string.Empty;
    }

    public class UpdatePinDto
    {
        public string answer { get; set; } = string.Empty;
        [Required]
        [RegularExpression(@"^[0-9]{4}$", ErrorMessage = "Pin must not be more than 4 digits")]
        public string oldPin { get; set; } = string.Empty;
        
        [Required]
        [RegularExpression(@"^[0-9]{4}$", ErrorMessage = "Pin must not be more than 4 digits")]
        public string newPin { get; set; } = string.Empty;
    }

    public class CreatePinDto
    {
        public string pin { get; set; } = string.Empty;
        public string confirmPin { get; set; } = string.Empty;
    }

    public class CreatePinViewModel
    {
        public bool status { get; set; }
        public string message { get; set; } = string.Empty;
    }

    public class UpdatePinViewModel
    {
        public bool status { get; set; }
        public bool isLocked { get; set; }
        public string message { get; set; } = string.Empty;
    }

    public class UserProfileViewModel
    {
        public string firstName { get; set; } = string.Empty;
        public string lastName { get; set; } = string.Empty;
        public string username { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string phoneNumber { get; set; } = string.Empty;
        public string address { get; set; } = string.Empty;
    }

    public class RequestDto
    {
        public decimal amount { get; set; }
    }

    public class PayStackResponseViewModel
    {
        public bool status { get; set; }
        public string message { get; set; } = string.Empty;
        public Data data { get; set; }
    }

    public class Data
    {
        public string authorization_url { get; set; } = string.Empty;
        public string access_code { get; set; } = string.Empty;
        public string reference { get; set; } = string.Empty;
    }

    public class CreateStatementViewModel
    {
        public bool status { get; set; }
        public string message { get; set; } = string.Empty;
    }

    public class CreateStatementRequestDTO
    {
        public string accountCurrency { get; set; } = string.Empty;
        public string startDate { get; set; } = string.Empty;
        public string endDate { get; set; } = string.Empty;
    }

    public class AccountDetails
    {
        public string AccountNumber { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string Currency { get; set; } = string.Empty;
    }

    public class AdminViewModel
    {
        public bool status { get; set; }
        public string message { get; set; } = string.Empty;
    }

    public class CreateAdminDTO
    {
        public string username { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
    }

    public class AdminLoginDTO
    {
        public string username { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
    }

    public class AdminLoginViewModel
    {
        public bool status { get; set; }
        public bool isSecure { get; set; }
        public string result { get; set; } = string.Empty;
        public string adminUsername { get; set; } = string.Empty;
        public string role { get; set; } = string.Empty;
        public string adminRefreshedToken { get; set; } = string.Empty;
    }

    public class ChangeAdminPasswordDTO 
    {
        public string password { get; set; } = string.Empty;
        public string confirmPassword { get; set; } = string.Empty;
    }

    public class KycViewModel
    {
        public bool status { get; set; }
        public string message { get; set; } = string.Empty;
    }

    public class KycAdminViewModel2
    {
        public string firstname { get; set; } = string.Empty;
        public string lastname { get; set; } = string.Empty;
        public List<KycAdminViewModel> list { get; set; }
    }

    public class KycAdminViewModel
    {
        public string image { get; set; } = string.Empty;
        public string filename { get; set; } = string.Empty;
        public string filecode { get; set; } = string.Empty;
        public int userId { get; set; }
        public bool isAccepted { get; set; }
        public DateTime timeUploaded { get; set; }
    }

    public class UserInfoOnKycUploadsForAdminModel
    {
        public string firstname { get; set; } = string.Empty;
        public string lastname { get; set; } = string.Empty;
        public int id { get; set; }
    }

    public class UserProfileModel
    {
        public bool status { get; set; }
        public string message { get; set; } = string.Empty;
    }

    public class CreateWalletDTO
    {
        public string currency { get; set; } = string.Empty;
    }

    public class CreateWalletModel
    {
        public bool status { get; set; }
        public string currency { get; set; } = string.Empty;
        public string message { get; set; } = string.Empty;
    }

    public class AccountDetailsModel
    {
        public List<AccountDetails> AccountDetails { get; set; }
    }

    public class CurrencyChargeModel
    {
        public bool status { get; set; }
        public decimal dollar { get; set; }
        public decimal euro { get; set; }
        public decimal pounds { get; set; }
    }

    public class AvailableCurrenciesModel
    {
        public string currency { get; set; } = string.Empty;
    }

    public class CreateSystemAccountsModel
    {
        public bool status { get; set; }
        public string message { get; set; } = string.Empty;
    }

    public class CreateSystemAccountsDTO
    {
        public string currency { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
    }

    public class UpdateChargeOrRateDTO
    {
        public string currency { get; set; } = string.Empty;
        public string action { get; set; } = string.Empty;
        public decimal amount { get; set; }
    }

    public class UpdateChargeOrRateModel
    {
        public bool status { get; set; }
        public string message { get; set; } = string.Empty;
    }

    public class GetNotificationModel
    {
        public int id { get; set; }
        public string username { get; set; } = string.Empty;
        public string heading { get; set; } = string.Empty;
        public string summary { get; set; } = string.Empty;
        public DateTime date { get; set; } 
        public bool isRead { get; set; }
    }

    public class GetUnreadNotificationDTO
    {
        public int id { get; set; } 
    }

    public class GetUnreadNotificationModel
    {
        public int id { get; set; }
        public string heading { get; set; } = string.Empty;
    }

    public class GetAllUnreadNotificationsNoModel
    {
        public int allNotifications { get; set; }
    }

    public class FundWalletDTO
    {
        public string currency { get; set; } = string.Empty;
        public decimal amount { get; set; }
    }

    public class FundWalletModel
    {
        public bool status { get; set; }
        public string message { get; set; } = string.Empty;
    }

    public class NairaBalModel
    {
        public decimal nairaBal { get; set; }
    }

    public class UserDataForAdminModel
    {
        public bool status { get; set; }
        public int verified { get; set; }
        public int unVerified { get; set; }
        public int locked { get; set; }
        public int unLocked { get; set; }
    }

    public class SystemAccountDetailsModel
    {
        public List<SystemAccountDetails> AccountDetails { get; set; }
    }

    public class SystemTransactionListModel
    {
        public decimal amount { get; set; }
        public string narration { get; set; } = string.Empty;
        public string walletAccount { get; set; } = string.Empty;
        public string systAccountNumber { get; set; } = string.Empty;
        public string transactionType { get; set; } = string.Empty;
        public decimal? rate { get; set; }
        public DateTime date { get; set; }
    }

    public class SystemAccountDetails
    {
        public string Name { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string Currency { get; set; } = string.Empty;
    }

    public class SendMoneyCheckModel
    {
        public string senderAccountNumber { get; set; } = string.Empty;
    }

    public class LockOrUnlockUsersDTO
    {
        public string username { get; set; } = string.Empty;
        public string action { get; set; } = string.Empty;
        public string code { get; set; } = string.Empty;
        public string reason { get; set; } = string.Empty;
    }

    public class LockOrUnlockUsersModel
    {
        public bool status { get; set; }
        public string message { get; set; } = string.Empty;
    }

    public class LockedUsersListModel
    {
        public string username { get; set; } = string.Empty;
        public string reason { get; set; } = string.Empty;
        public string code { get; set; } = string.Empty;
    }

    public class DisableEnableAdminDTO
    {
        public int id { get; set; }
    }

    public class AllAdminsListModel
    {
        public string username { get; set; } = string.Empty;
        public string role { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public bool isDisabled { get; set; }
        public int id { get; set; }
    }

    public class ReturnedModel
    {
        public string id { get; set; } = string.Empty;
        public string username { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string connId { get; set; } = string.Empty;
    }

    public class IsLoggedInModelAdmin
    {
        public int count { get; set; }
        public List<ReturnedModel> returneds { get; set; }
    }

    public class KycDocs
    {
        public string name { get; set; } = string.Empty;
        public string code { get; set; } = string.Empty;
    }





    public class CustomClaims
    {
        public const string UserId = "http://schemas.microsoft.com/ws/2008/06/identity/claims/userid";
        public const string UserName = "http://schemas.microsoft.com/ws/2008/06/identity/claims/username";
        public const string AccountNumber = "http://schemas.microsoft.com/ws/2008/06/identity/claims/accountnumber";
        public const string FirstName = "http://schemas.microsoft.com/ws/2008/06/identity/claims/firstname";
        public const string Role = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
    }

    public class AdminCustomClaims
    {
        public const string UserName = "http://schemas.microsoft.com/ws/2008/06/identity/claims/username";
        public const string Role = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
    }
   
}
