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
        public async Task<IActionResult> InitializePaystackPayment()
        {
            var result = await _paystackService.InitializePaystackPayment();
            return Redirect(result);
        }

        //[HttpPost("WebHook")]
        //[ProducesResponseType(200)]
        //[ProducesResponseType(400)]
        //[ProducesResponseType(404)]
        //public async Task<IActionResult> WebHookPaystack(WebHookViewModel WebHookViewModel)
        //{
        //    var result = await _paystackService.WebHookPaystack();
        //    return Ok("hook reached");
        //}

        //[HttpGet("VerifyPaystackPayment"), Authorize]



    }
}





