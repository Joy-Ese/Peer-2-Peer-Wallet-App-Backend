using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using WalletPayment.Services.Interfaces;
using WalletPayment.Services.Data;
using WalletPayment.Models.DataObjects;
using System.Net;

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

        public async Task<string> InitializePaystackPayment()
        {
            try
            {
                PayStackRequestDto requestModel = new PayStackRequestDto();
                PayStackResponseViewModel model = new PayStackResponseViewModel();

                string referenceString = ReferenceGenerator();

                requestModel.amount = requestModel.amount * 100;
                var secKey = string.Format("Bearer", _configuration.GetSection("SecretKey").Value);
                requestModel.email = //how to assign user email to this variable????;

                requestModel.reference = referenceString;

                var paystackApi = "https://api.paystack.co/transaction/initialize";

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                var http = (HttpWebRequest)WebRequest.Create(new Uri(paystackApi));
                http.Headers.Add("Authorization", secKey);
                http.Accept = "application/json";
                http.ContentType = "application/json";
                http.Method = "POST";



                return model.data.authorization_url;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return string.Empty;
            }
        }

        //public async Task WebHookPaystack(WebHookViewModel WebHookViewModel)
        //{
        //    try
        //    {
        //        Stream s = _httpContextAccessor.HttpContext.Request.Body;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
        //    }
        //}

    }
}







