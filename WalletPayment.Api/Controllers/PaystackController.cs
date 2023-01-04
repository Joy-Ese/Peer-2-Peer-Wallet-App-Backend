using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WalletPayment.Models.DataObjects;
using WalletPayment.Services.Interfaces;

namespace WalletPayment.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaystackController : ControllerBase
    {
        private readonly DataContext _context;
        private IPaystack _paystackService;

        public PaystackController(DataContext context, IPaystack paystackService)
        {
            _context = context;
            _paystackService = paystackService;

        }

        [HttpPost("InitializePaystackPayment"), Authorize]
        public async Task<IActionResult> InitializePaystackPayment(RequestDto req)
        {
            var result = await _paystackService.InitializePaystackPayment(req);
            return Ok(result);
        }

        [HttpPost("WebHook")]
        public async Task<IActionResult> WebHookPaystack()
        {
            await _paystackService.WebHookPaystack();
            return Ok();
        }

        [HttpGet("VerifyPaystackPayment"), Authorize]
        public async Task<IActionResult> VerifyPaystackPayment()
        {
            var result = await _paystackService.VerifyPaystackPayment();
            return Ok(result);
        }
    }
}





