using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using WalletPayment.Services.Interfaces;
using WalletPayment.Services.Data;
using WalletPayment.Models.DataObjects;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using RestSharp;
using Stripe;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using WalletPayment.Models.DataObjects.Common;
using WalletPayment.Models.Entites;

namespace WalletPayment.Services.Services
{
    public class PaymentService : IPayment
    {
        private readonly DataContext _context;
        private readonly IEmail _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<PaymentService> _logger;
        private readonly IConfiguration _configuration;
        private readonly PaystackDetails? _gatewayDetails;

        public PaymentService(DataContext context, IEmail emailService, IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor, ILogger<PaymentService> logger)
        {
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
            _gatewayDetails = configuration.GetSection("PaystackDetails").Get<PaystackDetails>();
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _logger.LogDebug(1, "Nlog injected into DashboardService");
        }

        private string ReferenceGenerator()
        {
            var referenceString = $"{Guid.NewGuid().ToString().Replace("-", "").Substring(1, 15)}";
            return referenceString;
        }

        public async Task<PayStackResponseViewModel> InitializePaystackPayment(RequestDto req)
        {
            PayStackResponseViewModel model = new PayStackResponseViewModel();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return model;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);
                var userInfo = await _context.Users.Where(u => u.Id == userID).FirstOrDefaultAsync();
                

                string referenceString = ReferenceGenerator();
                var secKey = _gatewayDetails?.SecretKey;

                if (req.amount <= 0)
                {
                    model.status = false;
                    model.message = "Transfer amount cannot be zero or negative";
                    return model;
                }

                int newAmt = (int)req.amount * 100;

                var postData = new
                {
                    email = userInfo.Email,
                    amount = newAmt,
                    currency = "NGN",
                    reference = referenceString,
                };

                var client = new RestClient(_gatewayDetails.PaystackInitializeApi);

                var request = new RestRequest();
                request.Method = RestSharp.Method.Post;
                request.AddJsonBody(JsonConvert.SerializeObject(postData));
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Authorization", $"Bearer {secKey}");

                RestResponse response = await client.ExecuteAsync(request);

                var result = JsonConvert.DeserializeObject<PayStackResponseViewModel>(response.Content);

                Deposit depositDetails = new Deposit
                {
                    Status = StatusMessage.Pending.ToString(),
                    Reference = referenceString,
                    Amount = req.amount,
                    Currency = "NGN",
                    UserId = userInfo.Id,
                    Date = DateTime.Now
                };

                await _context.Deposits.AddAsync(depositDetails);
                var resultPaystack = await _context.SaveChangesAsync();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return model;
            }
        }

        public async Task<WebhookDTO> WebHookPaystack(WebhookDTO eventData)
        {
            WebhookDTO webHookEventViewModel = new WebhookDTO();
            try
            {
                var now = DateTime.Now;
                
                var depositInfo = await _context.Deposits.Include("User").
                                            Where(dInfo => dInfo.Reference == eventData.data.reference).FirstOrDefaultAsync();

                var user = await _context.Users.Include("UserAccount").
                                            Where(uInfo => uInfo.Id == depositInfo.UserId)
                                            .FirstOrDefaultAsync();

                var acctInfo = await _context.Accounts.Where(x => x.UserId == depositInfo.UserId && x.Currency == "NGN").FirstOrDefaultAsync();

                var sysAcct = await _context.SystemAccounts.Where(x => x.Currency == "NGN" && x.Id == 10).FirstOrDefaultAsync();

                //var txns = await _context.Transactions.FirstOrDefaultAsync();


                if (!(eventData.@event.ToLower() == "charge.success") || 
                    !(eventData.data.reference == depositInfo.Reference))
                {
                    depositInfo.Status = StatusMessage.Failed.ToString();
                    depositInfo.Date = now;
                }

                depositInfo.Status = StatusMessage.Successful.ToString();
                depositInfo.Date = now;

                if (depositInfo.Status == StatusMessage.Successful.ToString())
                {
                    Transaction saveDepositInTxnDb = new Transaction
                    {
                        Reference = depositInfo.Reference,
                        Status = StatusMessage.Successful.ToString(),
                        Currencies = depositInfo.Currency, 
                        Amount = depositInfo.Amount,
                        TranSourceAccount = null,
                        TranDestinationAccount = acctInfo.AccountNumber,
                        Date = depositInfo.Date,
                        SourceAccountUserId = null,
                        DestinationAccountUserId = depositInfo.UserId,
                    };

                    await _context.Transactions.AddAsync(saveDepositInTxnDb);
                }

                // Paystack credits naria GL(System Account)
                sysAcct.SystemBalance += depositInfo.Amount;

                // Save "Paystack credits naria GL(System Account)" details in SystemTransactions
                SystemTransaction paystackCreditSysAcct = new SystemTransaction
                {
                    Amount = depositInfo.Amount,
                    Narration = $"Paystack credited {sysAcct.Name} GL with {depositInfo.Amount}",
                    ConversionRate = null,
                    SystemAccount = sysAcct.AccountNumber,
                    TransactionType = "CREDIT",
                    Date = DateTime.Now,
                    WalletAccountUserId = depositInfo.UserId,
                };

                await _context.SystemTransactions.AddAsync(paystackCreditSysAcct);
                await _context.SaveChangesAsync();


                // Naira GL(System Account) credits user's naira account
                sysAcct.SystemBalance -= depositInfo.Amount;
                var newBalance = acctInfo.Balance + depositInfo.Amount;
                acctInfo.Balance = newBalance;
                await _context.SaveChangesAsync();

                // Save "Naira GL(System Account) credits user's naira account" details in SystemTransactions
                SystemTransaction saveSysAcctTxn = new SystemTransaction
                {
                    Amount = depositInfo.Amount,
                    Narration = $"Funded {depositInfo.User.Username} {depositInfo.Currency} wallet with {depositInfo.Amount}",
                    ConversionRate = null,
                    SystemAccount = sysAcct.AccountNumber,
                    TransactionType = "DEBIT",
                    Date = DateTime.Now,
                    WalletAccountUserId = depositInfo.UserId,
                };

                await _context.SystemTransactions.AddAsync(saveSysAcctTxn);
                await _context.SaveChangesAsync();

                //Desposit Email information to user email
                var selfEmail = user.Email;
                var selfName = user.FirstName;
                var selfAmount = depositInfo.Amount.ToString();
                var selfBalance = newBalance.ToString();
                var date3 = depositInfo.Date.ToLongDateString();
                var currency = "NGN";
                var acctNum = acctInfo.AccountNumber;

                _emailService.SendDepositEmail(selfEmail, selfName, selfAmount, selfBalance, date3, currency, acctNum);

                return webHookEventViewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return webHookEventViewModel;
            }
        }

    }
}







