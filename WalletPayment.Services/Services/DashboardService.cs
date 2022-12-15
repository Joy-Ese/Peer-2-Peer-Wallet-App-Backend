using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WalletPayment.Models.DataObjects;
using WalletPayment.Models.Entites;
using WalletPayment.Services.Data;
using WalletPayment.Services.Interfaces;

namespace WalletPayment.Services.Services
{
    public class DashboardService : IDashboard
    {
        private readonly DataContext _context;
        private readonly IUser _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(DataContext context, IUser userService, 
            IHttpContextAccessor httpContextAccessor, ILogger<DashboardService> logger)
        {
            _context = context;
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _logger.LogDebug(1, "Nlog injected into DashboardService");
        }

        public async Task<UserDashboardViewModel> GetUserDetails()
        {
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return new UserDashboardViewModel();
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var userData = await _context.Users
                                .Where(userInfo => userInfo.Id == userID)
                                .Select(userInfo => new UserDashboardViewModel
                                {
                                    Username = userInfo.Username,
                                    FirstName = userInfo.FirstName,
                                    LastName = userInfo.LastName,
                                    AccountNumber = userInfo.UserAccount.AccountNumber,
                                    Balance = userInfo.UserAccount.Balance.ToString(),
                                })
                                .FirstOrDefaultAsync();

                if (userData == null) return new UserDashboardViewModel();

                return userData;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return new UserDashboardViewModel();
            }
        }

        public async Task<UserBalanceViewModel> GetUserAccountBalance()
        {
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return new UserBalanceViewModel();
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var acctBalance = await _context.Users
                                .Where(userAccBal => userAccBal.Id == userID)
                                .Select(userAccBal => new UserBalanceViewModel
                                {
                                    Balance = userAccBal.UserAccount.Balance.ToString()
                                })
                                .FirstOrDefaultAsync();

                if (acctBalance == null) return new UserBalanceViewModel();

                return acctBalance;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return new UserBalanceViewModel();
            }
        }

        public async Task<UserDashboardViewModel> GetUserAccountNumber()
        {
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return new UserDashboardViewModel();
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var acctNumber = await _context.Users
                                .Where(userAccNum => userAccNum.Id == userID)
                                .Select(userAccNum => new UserDashboardViewModel
                                {
                                    Balance = userAccNum.UserAccount.AccountNumber,
                                })
                                .FirstOrDefaultAsync();

                if (acctNumber == null) return new UserDashboardViewModel();

                return acctNumber;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return new UserDashboardViewModel();
            }
        }

        public async Task<UserUpdateViewModel> UpdateUserProfile(UserUpdateDto request)
        {
            UserUpdateViewModel updatedUser = new UserUpdateViewModel();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return updatedUser;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var updatedUserProfile = await _context.Users
                    .Where(uProfile => uProfile.Id == userID)
                    .FirstOrDefaultAsync();

                if (updatedUserProfile == null) return updatedUser;

                if (!_userService.VerifyPinHash(request.oldPin, updatedUserProfile.PinHash, updatedUserProfile.PinSalt))
                {
                    updatedUser.message = "Old Pin does not match pin used during registration";
                    return updatedUser;
                }

                _userService.CreatePinHash(request.newPin, out byte[] pinHash, out byte[] pinSalt);

                updatedUserProfile.PinHash = pinHash;
                updatedUserProfile.PinSalt = pinSalt;

                
                await _context.SaveChangesAsync();

                updatedUser.status = true;
                updatedUser.message = "Pin changed successfully";
                return updatedUser;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                updatedUser.message = "An exception occured";
                return updatedUser;
            }
        }
    }
}





