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

    public class UserViewModel
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Pin { get; set; } = string.Empty;
        public string ConfirmPin { get; set; } = string.Empty;
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
        public List<AccountDetails> accountDetails { get; set; }
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

    public class EmailDto
    {
        public string to { get; set; } = string.Empty;
        public string body { get; set; } = string.Empty;
        public string subject { get; set; } = string.Empty;
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

    public class PayStackRequestDto
    {
        public string reference { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string currency { get; set; } = string.Empty;
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
        public string password { get; set; } = string.Empty;
    }

    public class AdminLoginDTO
    {
        public string username { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
    }

    public class AdminLoginViewModel
    {
        public bool status { get; set; }
        public bool isAdmin { get; set; }
        public string result { get; set; } = string.Empty;
        public string adminUsername { get; set; } = string.Empty;
        public string adminRefreshedToken { get; set; } = string.Empty;
    }

    public class KycRequestViewModel
    {
        public bool status { get; set; }
        public string message { get; set; } = string.Empty;
    }

    public class KycRequestDTO
    {
        public List<IFormFile> KycDetails { get; set; }
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
