using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using WalletPayment.Models.DataObjects;
using WalletPayment.Models.Entites;
using WalletPayment.Services.Data;
using WalletPayment.Services.Interfaces;

namespace WalletPayment.Services.Services
{
    public class AccountService : IAccount
    {
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AccountService> _logger;

        public AccountService(DataContext context, IHttpContextAccessor httpContextAccessor, ILogger<AccountService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _logger.LogDebug(1, "Nlog injected into AccountService");
        }
        public async Task<Account> GetByAccountNumber(string AccountNumber)
        {
            AccountViewModel getAccountInfo = new AccountViewModel();
            try
            {
                var account = _context.Accounts.Where(x => x.AccountNumber == AccountNumber).FirstOrDefault();
                if (account == null) return null;

                return account;

            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                _logger.LogInformation("The error occurred at",
                    DateTime.UtcNow.ToLongTimeString());
                return new Account();
            }
        }

        public async Task<AccountViewModel> AccountLookUp(string AccountNumber)
        {
            AccountViewModel result = new AccountViewModel();
            try
            {
                var userData = await _context.Users
                    .Where(userAcc => userAcc.UserAccount.AccountNumber == AccountNumber)
                    .SingleOrDefaultAsync();
                if (userData == null)
                    return result;

                result.firstName = userData.FirstName;
                result.lastName = userData.LastName;
                result.status = true;

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return result;
            }
        }
    }

}










