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
                var account = await _context.Accounts.Include("User").Where(x => x.AccountNumber == AccountNumber)
                    .FirstOrDefaultAsync();
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

        public async Task<AccountViewModel> AccountLookUp(string searchInfo)
        {
            AccountViewModel result = new AccountViewModel();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return new AccountViewModel();
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var getAcctList = new List<AccountDetails>();
                var accountDetails = new AccountDetails();
                var acctData = await _context.Accounts.Where(x => x.UserId == userID).ToListAsync();


                foreach (var item in acctData)
                {
                    accountDetails.AccountNumber = item.AccountNumber;
                    accountDetails.Balance = item.Balance;
                    accountDetails.Currency = item.Currency;
                    getAcctList.Add(accountDetails);
                }


                var userData = await _context.Users.Include("UserAccount")
                    .Where(x => x.Email == searchInfo || x.Username == searchInfo).SingleOrDefaultAsync();

                if (userData == null)
                {
                    var acctInfo = await _context.Accounts.Include("User").Where(x => x.AccountNumber == searchInfo).FirstOrDefaultAsync();
                    result.status = true;
                    result.accountDetails = getAcctList;
                    result.firstName = acctInfo.User.FirstName;
                    result.lastName = acctInfo.User.LastName;
                }
                else
                {
                    result.status = true;
                    result.accountDetails = getAcctList;
                    result.firstName = userData.FirstName;
                    result.lastName = userData.LastName;
                }

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










