using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WalletPayment.Models.DataObjects;
using WalletPayment.Services.Data;
using WalletPayment.Services.Interfaces;

namespace WalletPayment.Services.Services
{
    public class DashboardService : IDashboard
    {
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(DataContext context, IHttpContextAccessor httpContextAccessor, ILogger<DashboardService> logger)
        {
            _context = context;
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
                _logger.LogInformation("The error occurred at",
                    DateTime.UtcNow.ToLongTimeString());
                return new UserDashboardViewModel();
            }
        }
    }
}





