using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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

                //string accNum;
                //if (_httpContextAccessor.HttpContext == null)
                //{
                //    return new Account();
                //}

                //accNum = _httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.AccountNumber)?.Value;


                //var userData = await _context.Users
                //                .Where(userAccInfo => userAccInfo.Equals(_context.Accounts.) == AccountNumber)
                //                .Select(userAccInfo => new AccountViewModel
                //                {
                //                    FirstName = userAccInfo.FirstName,
                //                    LastName = userAccInfo.LastName
                //                })
                //                .FirstOrDefaultAsync();

                //if (userData == null) return new Account();

                //return userData;




                var account = _context.Accounts.Where(x => x.AccountNumber == AccountNumber).FirstOrDefault();
                if (account == null) return null;

                return account;




                //var accountData = await _context.Accounts.Where(
                //                getData => getData.AccountNumber == AccountNumber).
                //                Select(getAccountData => new AccountDto
                //                {
                //                    FirstName = getAccountData.User.FirstName,
                //                    LastName = getAccountData.User.LastName,
                //                }).FirstOrDefaultAsync();


                //if (accountData == null) return new Account();
                //return accountData;

            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                _logger.LogInformation("The error occurred at",
                    DateTime.UtcNow.ToLongTimeString());
                return new Account();
            }
        }
    }

}










