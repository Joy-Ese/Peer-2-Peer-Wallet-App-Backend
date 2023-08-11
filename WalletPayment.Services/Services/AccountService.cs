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

        public async Task<UpdateChargeOrRateModel> UpdateChargeOrRate(UpdateChargeOrRateDTO req)
        {
            UpdateChargeOrRateModel resp = new UpdateChargeOrRateModel();
            try
            {
                string getRole;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return resp;
                }

                getRole = _httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.Role)?.Value;

                if (string.IsNullOrEmpty(req.currency) || string.IsNullOrEmpty(req.action))
                {
                    resp.message = "Input fields cannot be empty";
                    return resp;
                }

                if (req.action == "Rate")
                {
                    var rate = await _context.Currencies.Where(x => x.Currencies == req.currency).FirstOrDefaultAsync();

                    rate.ConversionRate = req.amount;
                    await _context.SaveChangesAsync();

                    resp.status = true;
                    resp.message = "Conversion rate successfully updated!";
                    return resp;
                }

                var charge = await _context.Currencies.Where(x => x.Currencies == req.currency).FirstOrDefaultAsync();

                charge.CurrencyCharge = req.amount;
                await _context.SaveChangesAsync();

                resp.status = true;
                resp.message = "Currency charge successfully updated!";
                return resp;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                resp.message = "An exception occured";
                return resp;
            }
        }

        public async Task<CreateSystemAccountsModel> CreateSystemAccount(CreateSystemAccountsDTO req)
        {
            CreateSystemAccountsModel resp = new CreateSystemAccountsModel();
            try
            {
                string getRole;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return resp;
                }

                getRole = _httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.Role)?.Value;

                var accountNum1 = DateTime.Now.ToString("yyyyMM");
                var accountNum2 = DateTime.Now.ToString("ddHH");

                SystemAccount systemAccount = new SystemAccount
                {
                    Name = $"{req.currency}{req.name}",
                    AccountNumber = $"{accountNum1}{req.currency}{accountNum2}",
                    SystemBalance = 0,
                    Currency = req.currency,
                };
                await _context.SystemAccounts.AddAsync(systemAccount);
                await _context.SaveChangesAsync();

                resp.status = true;
                resp.message = $"{systemAccount.Name} GL successfully created with account number : {systemAccount.AccountNumber}";
                return resp;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                resp.message = "An exception occured";
                return resp;
            }
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

        public async Task<AccountViewModel> AccountLookUp(string searchInfo, string currency)
        {
            AccountViewModel result = new AccountViewModel();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return result;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var accountDetailsOfUserSearched = await _context.Accounts.Include("User").Where(x => x.AccountNumber == searchInfo
                            || x.User.Username == searchInfo || x.User.Email == searchInfo).ToListAsync();
                
                var accountRequested = accountDetailsOfUserSearched.Where(x => x.Currency == currency).FirstOrDefault();

                //var getAcctList = new List<AccountDetails>();

                //foreach (var item in accountDetailsOfUserSearched)
                //{
                //    var accountDetails = new AccountDetails()
                //    {
                //        AccountNumber = item.AccountNumber,
                //        Balance = item.Balance,
                //        Currency = item.Currency,
                //    };
                //    getAcctList.Add(accountDetails);
                //}

                //if (accountDetailsOfUserSearched == null)
                //{
                //    result.status = true;
                //    result.accountDetails = getAcctList;
                //    result.firstName = getUserInfo.FirstName;
                //    result.lastName = getUserInfo.LastName;
                //}
                //else
                //{
                //    result.status = true;
                //    result.accountDetails = getAcctList;
                //    result.firstName = getUserInfo.FirstName;
                //    result.lastName = getUserInfo.LastName;
                //}

                var getUserInfo = await _context.Users.Where(x => x.Username == searchInfo).FirstOrDefaultAsync();

                if (getUserInfo == null) return result;

                if (accountRequested == null)
                {
                    result.status = false;
                    result.firstName = getUserInfo.FirstName;
                    return result;
                }

                result.status = true;
                result.acctNum = accountRequested.AccountNumber;
                result.firstName = accountRequested.User.FirstName;
                result.lastName = accountRequested.User.LastName;


                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return result;
            }
        }

        public async Task<SendMoneyCheckModel> SendMoneyCheck(string currency)
        {
            SendMoneyCheckModel response = new SendMoneyCheckModel();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return response;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var acctForSending = await _context.Accounts.Where(x => x.Currency == currency && x.UserId == userID)
                    .Select(x => x.AccountNumber).FirstOrDefaultAsync();

                if (acctForSending == null) return response;

                response.senderAccountNumber = acctForSending;
                return response;
            }
            catch(Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return response;
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

                var updateNewSysBal = await _context.SystemAccounts.Where(x => x.Currency == "NGN" && x.Id == 2).FirstOrDefaultAsync();
                var updateNewUserBal = await _context.Accounts.Where(x => x.UserId == userID && x.Currency == "NGN").FirstOrDefaultAsync();

                var userLoggedInAcctNum = await _context.Accounts.Where(x => x.UserId == userID && x.Currency == "NGN").Select(x => x.AccountNumber).FirstOrDefaultAsync();
                var speltCurrency = string.Empty;
                var frontendCurrency = string.Empty;
                if (req.currency == "USD")
                {
                    speltCurrency = "dollar";
                    frontendCurrency = "USD";

                    if (nairaBalance <= 500)
                    {
                        result.status = false;
                        result.message = $"Hi {userLoggedIn.Username}, you don't have sufficient balance to create your {speltCurrency} wallet";
                        return result;
                    }

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
                    systemTransaction.Amount = dollarCurrencyCharge;
                    systemTransaction.Narration = $"Charged {userLoggedIn.Username} for {speltCurrency} wallet creation";
                    systemTransaction.ConversionRate = null;
                    systemTransaction.SystemAccount = updateNewSysBal.AccountNumber;
                    systemTransaction.TransactionType = "CREDIT";
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
                    transaction.Currencies = "NGN";

                    await _context.Transactions.AddAsync(transaction);
                    await _context.SaveChangesAsync();
                }

                if (req.currency == "EUR")
                {
                    speltCurrency = "euro";
                    frontendCurrency = "EUR";

                    if (nairaBalance <= 1000)
                    {
                        result.status = false;
                        result.message = $"Hi {userLoggedIn.Username}, you don't have sufficient balance to create your {speltCurrency} wallet";
                        return result;
                    }

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
                    systemTransaction.Amount = euroCurrencyCharge;
                    systemTransaction.Narration = $"Charged {userLoggedIn.Username} for {speltCurrency} wallet creation";
                    systemTransaction.ConversionRate = null;
                    systemTransaction.SystemAccount = updateNewSysBal.AccountNumber;
                    systemTransaction.TransactionType = "CREDIT";
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
                    transaction.Currencies = "NGN";

                    await _context.Transactions.AddAsync(transaction);
                    await _context.SaveChangesAsync();
                }

                if (req.currency == "GBP")
                {
                    speltCurrency = "pounds";
                    frontendCurrency = "GBP";

                    if (nairaBalance <= 2000)
                    {
                        result.status = false;
                        result.message = $"Hi {userLoggedIn.Username}, you don't have sufficient balance to create your {speltCurrency} wallet";
                        return result;
                    }

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
                    systemTransaction.Amount = poundsCurrencyCharge;
                    systemTransaction.Narration = $"Charged {userLoggedIn.Username} for {speltCurrency} wallet creation";
                    systemTransaction.ConversionRate = null;
                    systemTransaction.SystemAccount = updateNewSysBal.AccountNumber;
                    systemTransaction.TransactionType = "CREDIT";
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
                    transaction.Currencies = "NGN";

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

                await _emailService.SendCreateWalletEmail(recepientEmail, amount, currency, firstName, date, balance);

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

        public async Task<List<AvailableCurrenciesModel>> CurrenciesSeededInDb()
        {
            List<AvailableCurrenciesModel> currencyList = new List<AvailableCurrenciesModel>();
            try
            {
                var currenciesSeededInDb = _context.Currencies.Select(x => new AvailableCurrenciesModel
                {
                    currency = x.Currencies
                }).ToList();

                return currenciesSeededInDb;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return currencyList;
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

        public async Task<List<AvailableCurrenciesModel>> FundWalletCurrencies()
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

                var userNGNCurrency = await _context.Accounts.Where(x => x.UserId == userID && x.Currency == "NGN").Select(x => x.Currency).ToArrayAsync();


                var fundWalletCurrencies = totalCurrenciesUserHas.Except(userNGNCurrency)
                    .Select(x => new AvailableCurrenciesModel { currency = x }).ToList();

                return fundWalletCurrencies;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return currencyList;
            }
        }

        public async Task<CurrencyChargeModel> GetCurrencyCharges()
        {
            CurrencyChargeModel currencyCharge = new CurrencyChargeModel();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return currencyCharge;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var dollarCurrencyCharge = await _context.Currencies.Where(x => x.Currencies == "USD").Select(x => x.CurrencyCharge).FirstOrDefaultAsync();
                var euroCurrencyCharge = await _context.Currencies.Where(x => x.Currencies == "EUR").Select(x => x.CurrencyCharge).FirstOrDefaultAsync();
                var poundsCurrencyCharge = await _context.Currencies.Where(x => x.Currencies == "GBP").Select(x => x.CurrencyCharge).FirstOrDefaultAsync();


                currencyCharge.status = true;
                currencyCharge.dollar = dollarCurrencyCharge;
                currencyCharge.euro = euroCurrencyCharge;
                currencyCharge.pounds = poundsCurrencyCharge;
                return currencyCharge;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return currencyCharge;
            }
        }

        public async Task<CurrencyChargeModel> GetConversionRates()
        {
            CurrencyChargeModel conversionRates = new CurrencyChargeModel();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return conversionRates;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var dollarConversionRates = await _context.Currencies.Where(x => x.Currencies == "USD").Select(x => x.ConversionRate).FirstOrDefaultAsync();
                var euroConversionRates = await _context.Currencies.Where(x => x.Currencies == "EUR").Select(x => x.ConversionRate).FirstOrDefaultAsync();
                var poundsConversionRates = await _context.Currencies.Where(x => x.Currencies == "GBP").Select(x => x.ConversionRate).FirstOrDefaultAsync();


                conversionRates.status = true;
                conversionRates.dollar = dollarConversionRates;
                conversionRates.euro = euroConversionRates;
                conversionRates.pounds = poundsConversionRates;
                return conversionRates;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return conversionRates;
            }
        }

        public async Task<List<AccountDetails>> UserAccountDetails()
        {
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return new List<AccountDetails>();
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var getAcctList = new List<AccountDetails>();

                var acctData = await _context.Accounts.Where(x => x.UserId == userID).ToListAsync();


                foreach (var item in acctData)
                {
                    var accountDetails = new AccountDetails()
                    {
                        AccountNumber = item.AccountNumber,
                        Balance = item.Balance,
                        Currency = item.Currency,
                    };
                    getAcctList.Add(accountDetails);
                }

                var userAcctDetails = await _context.Accounts
                                    .Where(x => x.UserId == userID)
                                    .Select(x => new AccountDetailsModel
                                    {
                                        AccountDetails = getAcctList
                                    }).ToListAsync();

                if (userAcctDetails == null) return new List<AccountDetails>();

                return getAcctList;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return new List<AccountDetails>();
            }
        }

        public async Task<NairaBalModel> GetNairaBalance()
        {
            NairaBalModel nairaBal = new NairaBalModel();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return nairaBal;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var nairaBalUserLoggedIn = await _context.Accounts.Where(x => x.UserId == userID && x.Currency == "NGN")
                    .Select(x => x.Balance).FirstOrDefaultAsync();

                nairaBal.nairaBal = nairaBalUserLoggedIn;
                return nairaBal;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return nairaBal;
            }
        }

        public async Task<FundWalletModel> FundForeignWallet(FundWalletDTO req)
        {
            FundWalletModel result = new FundWalletModel();
            SystemTransaction systemTransaction1 = new SystemTransaction();
            SystemTransaction systemTransaction2 = new SystemTransaction();
            Transaction transaction1 = new Transaction();
            Transaction transaction2 = new Transaction();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return result;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                if (string.IsNullOrEmpty(req.currency) || req.amount <= 0)
                {
                    result.message = "Transfer amount cannot be zero and currency cannot be empty";
                    return result;
                }

                var userLoggedIn = await _context.Users.Where(x => x.Id == userID).FirstOrDefaultAsync();
                var nairaBalUserLoggedIn = await _context.Accounts.Where(x => x.UserId == userID && x.Currency == "NGN").Select(x => x.Balance).FirstOrDefaultAsync();

                var dollarConversionRate = await _context.Currencies.Where(x => x.Currencies == "USD").Select(x => x.ConversionRate).FirstOrDefaultAsync();
                var euroConversionRate = await _context.Currencies.Where(x => x.Currencies == "EUR").Select(x => x.ConversionRate).FirstOrDefaultAsync();
                var poundsConversionRate = await _context.Currencies.Where(x => x.Currencies == "GBP").Select(x => x.ConversionRate).FirstOrDefaultAsync();

                var speltCurrency = string.Empty;
                decimal nairaEquivalent = 0;
                if (req.currency == "USD")
                {
                    speltCurrency = "dollar";
                    nairaEquivalent = req.amount * dollarConversionRate;
                }
                if (req.currency == "EUR")
                {
                    speltCurrency = "euro";
                    nairaEquivalent = req.amount * euroConversionRate;
                }
                if (req.currency == "GBP")
                {
                    speltCurrency = "pounds";
                    nairaEquivalent = req.amount * poundsConversionRate;
                }


                if (nairaEquivalent > nairaBalUserLoggedIn)
                {
                    result.status = false;
                    result.message = $"Hi {userLoggedIn.Username}, your balance is insufficient to complete this funding.";
                    return result;
                }

                using var dbTransaction = _context.Database.BeginTransaction();

                var saveEquivalentInNairaGL = await _context.SystemAccounts.Where(x => x.Currency == "NGN" && x.Id == 2).FirstOrDefaultAsync();
                var updateNewSysBal = await _context.SystemAccounts.Where(x => x.Currency == req.currency).FirstOrDefaultAsync();
                var updateNewUserNairaBal = await _context.Accounts.Where(x => x.UserId == userID && x.Currency == "NGN").FirstOrDefaultAsync();
                var updateNewUserForeignBal = await _context.Accounts.Where(x => x.UserId == userID && x.Currency == req.currency).FirstOrDefaultAsync();

                updateNewUserNairaBal.Balance -= nairaEquivalent;
                saveEquivalentInNairaGL.SystemBalance += nairaEquivalent;
                updateNewSysBal.SystemBalance -= req.amount;
                updateNewUserForeignBal.Balance += req.amount;

                var res = await _context.SaveChangesAsync();
                if (!(res > 0))
                {
                    result.status = false;
                    result.message = $"Unable to fund {speltCurrency} wallet.";
                    return result;
                }

                var userLoggedInNairaAcctNum = await _context.Accounts.Where(x => x.UserId == userID && x.Currency == "NGN").Select(x => x.AccountNumber).FirstOrDefaultAsync();
                var userLoggedInForeignAcctNum = await _context.Accounts.Where(x => x.UserId == userID && x.Currency == req.currency).Select(x => x.AccountNumber).FirstOrDefaultAsync();

                // Save details in SystemTransactions for NGN
                systemTransaction1.Amount = nairaEquivalent;
                if (req.currency == "USD")
                {
                    systemTransaction1.Narration = $"Debited {userLoggedIn.Username} naira wallet " +
                    $"{nairaEquivalent} for {speltCurrency} wallet funding";
                    systemTransaction1.ConversionRate = dollarConversionRate;
                }
                if (req.currency == "EUR")
                {
                    systemTransaction1.Narration = $"Debited {userLoggedIn.Username} naira wallet " +
                    $"{nairaEquivalent} for {speltCurrency} wallet funding";
                    systemTransaction1.ConversionRate = euroConversionRate;
                }
                if (req.currency == "GBP")
                {
                    systemTransaction1.Narration = $"Debited {userLoggedIn.Username} naira wallet " +
                    $"{nairaEquivalent} for {speltCurrency} wallet funding";
                    systemTransaction1.ConversionRate = poundsConversionRate;
                }
                systemTransaction1.SystemAccount = saveEquivalentInNairaGL.AccountNumber;
                systemTransaction1.TransactionType = "CREDIT";
                systemTransaction1.Date = DateTime.Now;
                systemTransaction1.WalletAccountUserId = userLoggedIn.Id;

                await _context.SystemTransactions.AddAsync(systemTransaction1);
                await _context.SaveChangesAsync();

                // Save details in SystemTransactions for Foreign
                systemTransaction2.Amount = req.amount;
                systemTransaction2.Narration = $"Credited {userLoggedIn.Username} {speltCurrency} wallet with {req.amount}";
                if (req.currency == "USD")
                {
                    systemTransaction2.ConversionRate = dollarConversionRate;
                }
                if (req.currency == "EUR")
                {
                    systemTransaction2.ConversionRate = euroConversionRate;
                }
                if (req.currency == "GBP")
                {
                    systemTransaction2.ConversionRate = poundsConversionRate;
                }
                systemTransaction2.SystemAccount = updateNewSysBal.AccountNumber;
                systemTransaction2.TransactionType = "DEBIT";
                systemTransaction2.Date = DateTime.Now;
                systemTransaction2.WalletAccountUserId = userLoggedIn.Id;

                await _context.SystemTransactions.AddAsync(systemTransaction2);
                await _context.SaveChangesAsync();


                // Save details in user Transactions for NGN
                transaction1.TranSourceAccount = userLoggedInNairaAcctNum;
                transaction1.TranDestinationAccount = null;
                transaction1.Amount = nairaEquivalent;
                transaction1.Date = DateTime.Now;
                transaction1.SourceAccountUserId = userLoggedIn.Id;
                transaction1.DestinationAccountUserId = null;
                transaction1.Status = StatusMessage.Successful.ToString();
                transaction1.Currencies = "NGN";

                await _context.Transactions.AddAsync(transaction1);
                await _context.SaveChangesAsync();

                // Save details in user Transactions for Foreign
                transaction2.TranSourceAccount = null;
                transaction2.TranDestinationAccount = userLoggedInForeignAcctNum;
                transaction2.Amount = req.amount;
                transaction2.Date = DateTime.Now;
                transaction2.SourceAccountUserId = null;
                transaction2.DestinationAccountUserId = userLoggedIn.Id;
                transaction2.Status = StatusMessage.Successful.ToString();
                transaction2.Currencies = req.currency;

                await _context.Transactions.AddAsync(transaction2);
                await _context.SaveChangesAsync();


                // FundForeignWallet Email information
                var selfEmail = userLoggedIn.Email;
                var selfName = userLoggedIn.FirstName;
                var selfAmount = req.amount.ToString();
                var selfBalance = updateNewUserForeignBal.Balance.ToString();
                var date3 = systemTransaction2.Date.ToLongDateString();
                var currency = req.currency;
                var acctNum = updateNewUserForeignBal.AccountNumber;

                _emailService.SendDepositEmail(selfEmail, selfAmount, selfBalance, date3, currency, selfName, acctNum);

                dbTransaction.Commit();

                result.status = true;
                result.message = $"You've successfully funded your wallet with {req.amount} {speltCurrency}";
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return result;
            }
        }

        public async Task<UserDataForAdminModel> GetUserDataForAdmin()
        {
            UserDataForAdminModel userDataForAdmin = new UserDataForAdminModel();
            try
            {
                string getRole;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return userDataForAdmin;
                }

                getRole = _httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.Role)?.Value;

                var getVerified = await _context.Users.Where(x => x.UserProfile == "Verified").CountAsync();
                var getUnverified = await _context.Users.Where(x => x.UserProfile == "Unverified").CountAsync();
                var getLocked = await _context.Users.Where(x => x.IsUserLocked == true).CountAsync();
                var getUnlocked = await _context.Users.Where(x => x.IsUserLocked == false).CountAsync();


                userDataForAdmin.status = true;
                userDataForAdmin.verified = getVerified;
                userDataForAdmin.unVerified = getUnverified;
                userDataForAdmin.locked = getLocked;
                userDataForAdmin.unLocked = getUnlocked;
                return userDataForAdmin;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return userDataForAdmin;
            }
        }

        public async Task<List<SystemAccountDetails>> SystemAccountDetails()
        {
            try
            {
                string getRole;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return new List<SystemAccountDetails>();
                }

                getRole = _httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.Role)?.Value;

                var getAcctList = new List<SystemAccountDetails>();

                var sysAcctData = await _context.SystemAccounts.ToListAsync();


                foreach (var item in sysAcctData)
                {
                    var accountDetails = new SystemAccountDetails()
                    {
                        Name = item.Name,
                        AccountNumber = item.AccountNumber,
                        Balance = item.SystemBalance,
                        Currency = item.Currency,
                    };
                    getAcctList.Add(accountDetails);
                }

                var sysAcctDetails = await _context.SystemAccounts.Select(x => new SystemAccountDetailsModel
                {
                    AccountDetails = getAcctList
                }).ToListAsync();


                if (sysAcctDetails == null) return new List<SystemAccountDetails>();

                return getAcctList;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return new List<SystemAccountDetails>();
            }
        }

        public async Task<LockOrUnlockUsersModel> LockOrUnlockUsers(LockOrUnlockUsersDTO req)
        {
            LockOrUnlockUsersModel resp = new LockOrUnlockUsersModel();
            try
            {
                string getRole;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return resp;
                }

                getRole = _httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.Role)?.Value;

                if (string.IsNullOrEmpty(req.username) || string.IsNullOrEmpty(req.action))
                {
                    resp.message = "Input fields cannot be empty";
                    return resp;
                }

                var getUser = await _context.Users.Where(x => x.Username == req.username).FirstOrDefaultAsync();

                if (req.action == "Lock")
                {
                    getUser.IsUserLocked = true;
                    getUser.LockedReason = req.reason;
                    getUser.LockedReasonCode = req.code;
                    await _context.SaveChangesAsync();

                    resp.status = true;
                    resp.message = $"{req.username}'s account has successfully been locked!!";
                    return resp;
                }

                getUser.IsUserLocked = false;
                getUser.LockedReason = req.reason;
                getUser.LockedReasonCode = req.code;
                await _context.SaveChangesAsync();

                resp.status = true;
                resp.message = $"{req.username}'s account has successfully been unlocked!!";
                return resp;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                resp.message = "An exception occured";
                return resp;
            }
        }

        public async Task<List<LockedUsersListModel>> GetLockedUsersList()
        {
            List<LockedUsersListModel> lockedUsersList = new List<LockedUsersListModel>();
            try
            {
                string getRole;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return lockedUsersList;
                }

                getRole = _httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.Role)?.Value;

                var lockedUsers = await _context.Users.Where(x => x.IsUserLocked == true).ToListAsync();

                foreach (var locked in lockedUsers)
                {
                    lockedUsersList.Add(new LockedUsersListModel
                    {
                        username = locked.Username,
                        reason = locked.LockedReason,
                        code = locked.LockedReasonCode,
                    });
                }

                return lockedUsersList;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                _logger.LogInformation("The error occurred at",
                    DateTime.UtcNow.ToLongTimeString());
                return lockedUsersList;
            }
        }

        public async Task<IsLoggedInModelAdmin> GetAdminIsLoggedIn()
        {
            IsLoggedInModelAdmin admin = new IsLoggedInModelAdmin();
            try
            {
                var allUsersLoggedIn = await _context.Users.Where(x => x.IsUserLogin == true).ToListAsync();
                var allUsersLoggedInCount = allUsersLoggedIn.Count(); //await _context.Users.Where(x => x.IsUserLogin == true).CountAsync();

                var users = new List<ReturnedModel>();
                foreach (var item in allUsersLoggedIn)
                {
                    users.Add(new ReturnedModel
                    {
                        username = item.Username,
                        email = item.Email,
                        id = item.Id.ToString()
                    });
                }

                admin.count = allUsersLoggedInCount;
                admin.returneds = users;
                return admin;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return admin;
            }
        }

        public async Task<bool> IsUserLoggedIn()
        {
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return false;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var isUserLoggedIN = await _context.Users.Where(x => x.Id == userID).FirstOrDefaultAsync();

                return isUserLoggedIN.IsUserLogin;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return false;
            }
        }

        


    }

}










