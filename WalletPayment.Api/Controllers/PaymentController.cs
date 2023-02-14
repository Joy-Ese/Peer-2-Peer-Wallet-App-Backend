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

namespace WalletPayment.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly PaystackDetails? _gatewayDetails;
        private readonly DataContext _context;
        private IPayment _paymentService;

        public PaymentController(DataContext context, IPayment paystackService, IConfiguration configuration)
        {
            _configuration = configuration;
            _gatewayDetails = configuration.GetSection("PaystackDetails").Get<PaystackDetails>();
            _context = context;
            _paymentService = paystackService;

        }

        [HttpPost("InitializePaystackPayment"), Authorize]
        public async Task<IActionResult> InitializePaystackPayment(RequestDto req)
        {
            var result = await _paymentService.InitializePaystackPayment(req);
            return Ok(result);
        }

        [HttpPost("WebHook")]
        public async Task<IActionResult> WebHookPaystack()
        {
            try
            {
                if (_gatewayDetails?.WhitelistedPaystackIPs == null)
                    return BadRequest();

                var requestIp = HttpContext?.Connection?.RemoteIpAddress?.ToString();
                bool isAllowed = _gatewayDetails.WhitelistedPaystackIPs.Where(ip => IPAddress.Parse(ip).Equals(requestIp)).Any();

                if (!isAllowed)
                    return BadRequest();

                String secKey = _gatewayDetails.SecretKey;
                String result = "";

                var reqHeader = HttpContext.Request.Headers;

                byte[] secretkeyBytes = Encoding.UTF8.GetBytes(secKey);
                byte[] inputBytes = Encoding.UTF8.GetBytes(reqHeader.AcceptCharset);

                using (var hmac = new HMACSHA512(secretkeyBytes))
                {
                    byte[] hashValue = hmac.ComputeHash(inputBytes);
                    result = BitConverter.ToString(hashValue).Replace("-", string.Empty);
                }
                Console.WriteLine(result);

                String xpaystackSignature = "x-paystack-signature";

                var webHookEvent = JsonConvert.DeserializeObject<WebHookEventViewModel>(reqHeader.ToString());

                if (result.ToLower().Equals(xpaystackSignature))
                {
                    if (!(webHookEvent.@event == "charge.success"))
                    {
                        return BadRequest();
                    }
                }

                var bodyText = new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                var reqq = JsonConvert.DeserializeObject<WebHookEventViewModel>(bodyText.Result);

                await _paymentService.WebHookPaystack(reqq);
                return Ok();

            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
    }
}





