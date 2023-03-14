using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WalletPayment.Models.DataObjects;
using WalletPayment.Services.Interfaces;
using Newtonsoft.Json;
using System.Net;
using WalletPayment.Models.DataObjects.Common;
using System.Text;
using System.Security.Cryptography;
using Microsoft.Extensions.Primitives;

namespace WalletPayment.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly PaystackDetails? _gatewayDetails;
        private readonly DataContext _context;
        private readonly ILogger<PaymentController> _logger;
        private IPayment _paymentService;

        public PaymentController(DataContext context, IPayment paystackService, IConfiguration configuration, ILogger<PaymentController> logger)
        {
            _configuration = configuration;
            _gatewayDetails = configuration.GetSection("PaystackDetails").Get<PaystackDetails>();
            _context = context;
            _paymentService = paystackService;
            _logger = logger;
            _logger.LogDebug(1, "Nlog injected into PaymentController");

        }

        [HttpPost("InitializePaystackPayment"), Authorize]
        public async Task<IActionResult> InitializePaystackPayment(RequestDto req)
        {
            var result = await _paymentService.InitializePaystackPayment(req);
            return Ok(result);
        }

        [HttpPost("WebHook")]
        public async Task<IActionResult> WebHookPaystack(object obj)
        {
            try
            {
                var webHookEvent = JsonConvert.DeserializeObject<WebhookDTO>(obj.ToString());
                string secKey = _gatewayDetails.SecretKey;
                string result = "";

                var reqHeader = HttpContext.Request.Headers;
                var bodyText = new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

                byte[] secretkeyBytes = Encoding.UTF8.GetBytes(secKey);
                byte[] inputBytes = Encoding.UTF8.GetBytes(obj.ToString());

                using (var hmac = new HMACSHA512(secretkeyBytes))
                {
                    byte[] hashValue = hmac.ComputeHash(inputBytes);
                    result = BitConverter.ToString(hashValue).Replace("-", string.Empty);
                }
                Console.WriteLine(result);

                reqHeader.TryGetValue("x-paystack-signature", out StringValues xpaystackSignature);

                if (!result.ToLower().Equals(xpaystackSignature)) return BadRequest();

                await _paymentService.WebHookPaystack(webHookEvent);
                return Ok();

            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return StatusCode(500);
            }
        }
    }
}





