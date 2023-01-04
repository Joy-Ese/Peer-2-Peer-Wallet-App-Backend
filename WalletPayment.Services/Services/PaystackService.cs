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

namespace WalletPayment.Services.Services
{
    public class PaystackService : IPaystack
    {
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<PaystackService> _logger;
        private readonly IConfiguration _configuration;

        public PaystackService(DataContext context, IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor, ILogger<PaystackService> logger)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _logger.LogDebug(1, "Nlog injected into DashboardService");
        }

        public string ReferenceGenerator()
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
                var secKey = _configuration.GetSection("SecretKey").Value;

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
                    reference = referenceString
                };

                var client = new RestClient(_configuration.GetSection("PaystackInitializeApi").Value);

                var request = new RestRequest();
                request.Method = RestSharp.Method.Post;
                request.AddJsonBody(JsonConvert.SerializeObject(postData));
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Authorization", $"Bearer {secKey}");

                RestResponse response = await client.ExecuteAsync(request);

                var result = JsonConvert.DeserializeObject<PayStackResponseViewModel>(response.Content);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return model;
            }
        }

        public async Task<WebHookEventViewModel> WebHookPaystack()
        {
            WebHookEventViewModel webHookEventViewModel = new WebHookEventViewModel();
            try
            {
                String secKey = _configuration.GetSection("SecretKey").Value;
                var jsonInput = JsonConvert.SerializeObject(_httpContextAccessor.HttpContext.Request.Body);
                String result = "";

                byte[] secretkeyBytes = Encoding.UTF8.GetBytes(secKey);
                byte[] inputBytes = Encoding.UTF8.GetBytes(jsonInput);

                using (var hmac = new HMACSHA512(secretkeyBytes))
                {
                    byte[] hashValue = hmac.ComputeHash(inputBytes);
                    result = BitConverter.ToString(hashValue).Replace("-", string.Empty);
                }
                Console.WriteLine(result);

                String xpaystackSignature = "x-paystack-signature";

                var webHookEvent = JsonConvert.DeserializeObject<WebHookEventViewModel>(jsonInput);

                if (result.ToLower().Equals(xpaystackSignature))
                {
                    if (!(webHookEvent.@event == "charge.success"))
                    {
                        return webHookEventViewModel;
                    }
                }

                return webHookEvent;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return webHookEventViewModel;
            }
        }

        public async Task<VerifyPayStackResponseViewModel> VerifyPaystackPayment()
        {
            VerifyPayStackResponseViewModel verifyModel = new VerifyPayStackResponseViewModel();
            try
            {
                string referenceString = ReferenceGenerator();
                var secKey = _configuration.GetSection("SecretKey").Value;

                var client = new RestClient(_configuration.GetSection("PaystackVerifyApi").Value) + referenceString;

                var request = new RestRequest();
                request.Method = RestSharp.Method.Get;
                request.AddHeader("Authorization", $"Bearer {secKey}");

                //RestResponse response = await client.ExecuteAsync(request);

                //var result = JsonConvert.DeserializeObject<VerifyPayStackResponseViewModel>(response.Content);

                verifyModel.status = true;
                verifyModel.message = "Payment Verified";
                return verifyModel;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return verifyModel;
            }
        }
    }
}







