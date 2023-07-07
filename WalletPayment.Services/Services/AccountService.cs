using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using WalletPayment.Models.DataObjects;
using WalletPayment.Models.Entites;
using WalletPayment.Services.Data;
using WalletPayment.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace WalletPayment.Services.Services
{
    public class AccountService : IAccount
    {
        private readonly DataContext _context;
        private readonly IHubContext<NotificationSignalR> _hub;
        private readonly IEmail _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AccountService> _logger;

        public AccountService(DataContext context, IEmail emailService, IHttpContextAccessor httpContextAccessor, ILogger<AccountService> logger, IHubContext<NotificationSignalR> hub)
        {
            _context = context;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _logger.LogDebug(1, "Nlog injected into AccountService");
            _hub = hub;
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
                var userData = await _context.Users.Include("UserAccount")
                    .Where(x => x.Email == searchInfo || x.Username == searchInfo).SingleOrDefaultAsync();


                var getAcctList = new List<AccountDetails>();
                var accountDetails = new AccountDetails();
                var acctData = await _context.Accounts.Include("User").Where(x => x.AccountNumber == searchInfo 
                            || x.User.Username == searchInfo || x.User.Email == searchInfo).ToListAsync();

                foreach (var item in acctData)
                {
                    accountDetails.AccountNumber = item.AccountNumber;
                    accountDetails.Balance = item.Balance;
                    accountDetails.Currency = item.Currency;
                    getAcctList.Add(accountDetails);
                }


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

        public string AccountNumberGenerator()
        {
            var accountNumber = DateTime.Now.ToString("HHffMMffdd");
            return accountNumber;
        }

        public async Task<CreateWalletModel> CreateForeignWallet(CreateWalletDTO req)
        {
            CreateWalletModel result = new CreateWalletModel();
            SystemTransaction systemTransaction = new SystemTransaction();
            Transaction transaction = new Transaction();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return result;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var userLoggedIn = await _context.Users.Where(x => x.Id == userID).FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(req.currency))
                {
                    result.status = false;
                    result.message = $"Hi {userLoggedIn.Username}, please choose a currency";
                    return result;
                }

                if (userLoggedIn.UserProfile != "Verified")
                {
                    result.status = false;
                    result.message = $"Hi {userLoggedIn.Username}, please perform your KYC validation to enable foreign wallet creation";
                    return result;
                }

                var nairaBalance = await _context.Accounts.Where(x => x.UserId == userID && x.Currency == "NGN").Select(x => x.Balance).FirstOrDefaultAsync();
                if (nairaBalance <= 0)
                {
                    result.status = false;
                    result.message = $"Hi {userLoggedIn.Username}, please fund your account before creating foreign wallets";
                    return result;
                }

                var totalUserCurrencies = await _context.Accounts.Where(x => x.UserId == userID).Select(x => x.Currency).CountAsync();
                if (totalUserCurrencies == 4)
                {
                    result.status = false;
                    result.message = $"Hi {userLoggedIn.Username}, you cannot have more than 3 foreign wallets";
                    return result;
                }

                var totalCurrenciesUserHas = await _context.Accounts.Where(x => x.UserId == userID).Select(x => x.Currency).ToListAsync();
                if (totalCurrenciesUserHas.Contains(req.currency))
                {
                    result.status = false;
                    result.message = $"Hi {userLoggedIn.Username}, this wallet already exist";
                    return result;
                }

                var dollarCurrencyCharge = await _context.Currencies.Where(x => x.Currencies == "USD").Select(x => x.CurrencyCharge).FirstOrDefaultAsync();
                var euroCurrencyCharge = await _context.Currencies.Where(x => x.Currencies == "EUR").Select(x => x.CurrencyCharge).FirstOrDefaultAsync();
                var poundsCurrencyCharge = await _context.Currencies.Where(x => x.Currencies == "GBP").Select(x => x.CurrencyCharge).FirstOrDefaultAsync();


                var systemBal = await _context.SystemAccounts.Select(x => x.SystemBalance).FirstOrDefaultAsync();

                using var dbTransaction = _context.Database.BeginTransaction();

                string generatedAcc = AccountNumberGenerator();

                Account newAccount = new Account
                {
                    AccountNumber = generatedAcc,
                    Balance = 0,
                    Currency = req.currency,
                    UserId = userID
                };

                await _context.Accounts.AddAsync(newAccount);
                await _context.SaveChangesAsync();

                var updateNewSysBal = await _context.SystemAccounts.FirstOrDefaultAsync();
                var updateNewUserBal = await _context.Accounts.Where(x => x.UserId == userID && x.Currency == "NGN").FirstOrDefaultAsync();

                var userLoggedInAcctNum = await _context.Accounts.Where(x => x.UserId == userID && x.Currency == "NGN").Select(x => x.AccountNumber).FirstOrDefaultAsync();
                var speltCurrency = string.Empty;
                var frontendCurrency = string.Empty;
                if (req.currency == "USD")
                {
                    speltCurrency = "dollar";
                    frontendCurrency = "USD";

                    updateNewSysBal.SystemBalance += dollarCurrencyCharge;
                    updateNewUserBal.Balance -= dollarCurrencyCharge;

                    var res = await _context.SaveChangesAsync();
                    if (!(res > 0))
                    {
                        result.status = false;
                        result.message = $"Failed to charge for {speltCurrency} wallet creation";
                        return result;
                    }

                    // Save details in SystemTransactions
                    systemTransaction.Status = StatusMessage.Successful.ToString();
                    systemTransaction.Amount = dollarCurrencyCharge;
                    systemTransaction.Narration = $"Charged {userLoggedIn.Username} for {speltCurrency} wallet creation";
                    systemTransaction.WalletUserAccount = userLoggedInAcctNum;
                    systemTransaction.Date = DateTime.Now;
                    systemTransaction.WalletAccountUserId = userLoggedIn.Id;

                    await _context.SystemTransactions.AddAsync(systemTransaction);
                    await _context.SaveChangesAsync();

                    // Save details in user Transactions 
                    transaction.TranSourceAccount = userLoggedInAcctNum;
                    transaction.TranDestinationAccount = null;
                    transaction.Amount = dollarCurrencyCharge;
                    transaction.Date = DateTime.Now;
                    transaction.SourceAccountUserId = userLoggedIn.Id;
                    transaction.DestinationAccountUserId = null;
                    transaction.Status = StatusMessage.Successful.ToString();

                    await _context.Transactions.AddAsync(transaction);
                    await _context.SaveChangesAsync();
                }

                if (req.currency == "EUR")
                {
                    speltCurrency = "euro";
                    frontendCurrency = "EUR";

                    updateNewSysBal.SystemBalance += euroCurrencyCharge;
                    updateNewUserBal.Balance -= euroCurrencyCharge;

                    var res = await _context.SaveChangesAsync();
                    if (!(res > 0))
                    {
                        result.status = false;
                        result.message = $"Failed to charge for {speltCurrency} wallet creation";
                        return result;
                    }

                    // Save details in SystemTransactions
                    systemTransaction.Status = StatusMessage.Successful.ToString();
                    systemTransaction.Amount = euroCurrencyCharge;
                    systemTransaction.Narration = $"Charged {userLoggedIn.Username} for {speltCurrency} wallet creation";
                    systemTransaction.WalletUserAccount = userLoggedInAcctNum;
                    systemTransaction.Date = DateTime.Now;
                    systemTransaction.WalletAccountUserId = userLoggedIn.Id;

                    await _context.SystemTransactions.AddAsync(systemTransaction);
                    await _context.SaveChangesAsync();

                    // Save details in user Transactions 
                    transaction.TranSourceAccount = userLoggedInAcctNum;
                    transaction.TranDestinationAccount = null;
                    transaction.Amount = euroCurrencyCharge;
                    transaction.Date = DateTime.Now;
                    transaction.SourceAccountUserId = userLoggedIn.Id;
                    transaction.DestinationAccountUserId = null;
                    transaction.Status = StatusMessage.Successful.ToString();

                    await _context.Transactions.AddAsync(transaction);
                    await _context.SaveChangesAsync();
                }

                if (req.currency == "GBP")
                {
                    speltCurrency = "pounds";
                    frontendCurrency = "GBP";

                    updateNewSysBal.SystemBalance += poundsCurrencyCharge;
                    updateNewUserBal.Balance -= poundsCurrencyCharge;

                    var res = await _context.SaveChangesAsync();
                    if (!(res > 0))
                    {
                        result.status = false;
                        result.message = $"Failed to charge for {speltCurrency} wallet creation";
                        return result;
                    }

                    // Save details in SystemTransactions
                    systemTransaction.Status = StatusMessage.Successful.ToString();
                    systemTransaction.Amount = poundsCurrencyCharge;
                    systemTransaction.Narration = $"Charged {userLoggedIn.Username} for {speltCurrency} wallet creation";
                    systemTransaction.WalletUserAccount = userLoggedInAcctNum;
                    systemTransaction.Date = DateTime.Now;
                    systemTransaction.WalletAccountUserId = userLoggedIn.Id;

                    await _context.SystemTransactions.AddAsync(systemTransaction);
                    await _context.SaveChangesAsync();

                    // Save details in user Transactions 
                    transaction.TranSourceAccount = userLoggedInAcctNum;
                    transaction.TranDestinationAccount = null;
                    transaction.Amount = poundsCurrencyCharge;
                    transaction.Date = DateTime.Now;
                    transaction.SourceAccountUserId = userLoggedIn.Id;
                    transaction.DestinationAccountUserId = null;
                    transaction.Status = StatusMessage.Successful.ToString();

                    await _context.Transactions.AddAsync(transaction);
                    await _context.SaveChangesAsync();
                }


                // CreateForeignWallet Email information
                var recepientEmail = userLoggedIn.Email;
                var amount = systemTransaction.Amount.ToString();
                var currency = speltCurrency;
                var firstName = userLoggedIn.FirstName;
                var date = systemTransaction.Date.ToLongDateString();
                var balance = updateNewUserBal.Balance.ToString();

                //await _emailService.SendCreateWalletEmail(recepientEmail, amount,currency, firstName, date, balance);

                dbTransaction.Commit();

                result.status = true;
                result.currency = frontendCurrency;
                result.message = $"Hi {userLoggedIn.Username}, you've successfully created a {speltCurrency} " +
                    $"wallet with this account number {generatedAcc}";
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return result;
            }
        }

        public async Task<List<AvailableCurrenciesModel>> UnavailableCurrencies()
        {
            List<AvailableCurrenciesModel> currencyList = new List<AvailableCurrenciesModel>();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return currencyList;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var totalCurrenciesUserHas = await _context.Accounts.Where(x => x.UserId == userID).Select(x => x.Currency).ToArrayAsync();

                var currenciesSeededInDb = await _context.Currencies.Select(x => x.Currencies).ToArrayAsync();


                var currenciesUserDoesNotHave = currenciesSeededInDb.Except(totalCurrenciesUserHas)
                    .Select(x => new AvailableCurrenciesModel { currency = x }).ToList();

                return currenciesUserDoesNotHave;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return currencyList;
            }
        }

        //public async Task<CurrencyChargeModel> GetCurrencyCharges()
        //{
        //    CurrencyChargeModel currencyCharge = new CurrencyChargeModel();
        //    try
        //    {
        //        int userID;
        //        if (_httpContextAccessor.HttpContext == null)
        //        {
        //            return currencyCharge;
        //        }

        //        userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

        //        var dollarCurrencyCharge = await _context.Currencies.Where(x => x.Currencies == "USD").Select(x => x.CurrencyCharge).FirstOrDefaultAsync();
        //        var euroCurrencyCharge = await _context.Currencies.Where(x => x.Currencies == "EUR").Select(x => x.CurrencyCharge).FirstOrDefaultAsync();
        //        var poundsCurrencyCharge = await _context.Currencies.Where(x => x.Currencies == "GBP").Select(x => x.CurrencyCharge).FirstOrDefaultAsync();


        //        currencyCharge.status = true;
        //        currencyCharge.dollar = dollarCurrencyCharge;
        //        currencyCharge.euro = euroCurrencyCharge;
        //        currencyCharge.pounds = poundsCurrencyCharge;
        //        return currencyCharge;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
        //        return currencyCharge;
        //    }
        //}

        //public async Task<List<AccountDetails>> UserAccountDetails()
        //{
        //    try
        //    {
        //        int userID;
        //        if (_httpContextAccessor.HttpContext == null)
        //        {
        //            return new List<AccountDetails>();
        //        }

        //        userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

        //        var getAcctList = new List<AccountDetails>();
                
        //        var acctData = await _context.Accounts.Where(x => x.UserId == userID).ToListAsync();


        //        foreach (var item in acctData)
        //        {
        //            var accountDetails = new AccountDetails()
        //            {
        //                AccountNumber = item.AccountNumber,
        //                Balance = item.Balance,
        //                Currency = item.Currency,
        //            };
        //            getAcctList.Add(accountDetails);
        //        }

        //        var userAcctDetails = await _context.Accounts
        //                            .Where(x => x.UserId == userID)
        //                            .Select(x => new AccountDetailsModel
        //                            {
        //                                AccountDetails = getAcctList
        //                            }).ToListAsync();

        //        if (userAcctDetails == null) return new List<AccountDetails>();

        //        return getAcctList;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
        //        return new List<AccountDetails>();
        //    }
        //}



    }

}










