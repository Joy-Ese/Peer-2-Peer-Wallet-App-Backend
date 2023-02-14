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
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<PaymentService> _logger;
        private readonly IConfiguration _configuration;
        private readonly PaystackDetails? _gatewayDetails;

        public PaymentService(DataContext context, IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor, ILogger<PaymentService> logger)
        {
            _context = context;
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

                PaystackDeposit paystackDepositDetails = new PaystackDeposit
                {
                    PaystackDepositStatus = StatusMessage.Pending.ToString(),
                    PaystackReference = referenceString,
                    PaystackEmail = userInfo.Email,
                    PaystackAmount = req.amount,
                    PaystackCurrency = "NGN",
                    UserId = userInfo.Id
                };

                await _context.PaystackDeposits.AddAsync(paystackDepositDetails);
                var resultPaystack = await _context.SaveChangesAsync();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return model;
            }
        }

        public async Task<WebHookEventViewModel> WebHookPaystack(WebHookEventViewModel eventData)
        {
            WebHookEventViewModel webHookEventViewModel = new WebHookEventViewModel();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return webHookEventViewModel;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);
                var paystackDepositInfo = await _context.PaystackDeposits.
                                            Where(pd => pd.Id == userID).FirstOrDefaultAsync();
                var userBal = await _context.Accounts.
                                            Where(a => a.Id == userID).FirstOrDefaultAsync();

                paystackDepositInfo.PaystackDepositStatus = StatusMessage.Successful.ToString();

                if (!(webHookEventViewModel.@event.ToLower() == "charge.success") || 
                    !(webHookEventViewModel.data.reference == paystackDepositInfo.PaystackReference))
                {
                    paystackDepositInfo.PaystackDepositStatus = StatusMessage.Failed.ToString();
                }

                var newBal = userBal.Balance + paystackDepositInfo.PaystackAmount;
                userBal.Balance = newBal;
                await _context.SaveChangesAsync();
                   
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







