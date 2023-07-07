using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WalletPayment.Models.DataObjects;
using WalletPayment.Models.Entites;
using WalletPayment.Services.Interfaces;

namespace WalletPayment.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private IAccount _accountService; 

        public AccountController(IAccount accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("AccountLookUp"), Authorize]
        public async Task<IActionResult> AccountLookUp(string searchInfo)
        {
            var result = await _accountService.AccountLookUp(searchInfo);
            return Ok(result);
        }

        [HttpPost("CreateForeignWallet"), Authorize]
        public async Task<IActionResult> CreateForeignWallet(CreateWalletDTO req)
        {
            var result = await _accountService.CreateForeignWallet(req);
            return Ok(result);
        }

        [HttpGet("UnavailableCurrencies"), Authorize]
        public async Task<IActionResult> UnavailableCurrencies()
        {
            var result = await _accountService.UnavailableCurrencies();
            return Ok(result);
        }

        //[HttpGet("UserAccountDetails"), Authorize]
        //public async Task<IActionResult> UserAccountDetails()
        //{
        //    var result = await _accountService.UserAccountDetails();
        //    return Ok(result);
        //}

        //[HttpGet("GetCurrencyCharges"), Authorize]
        //public async Task<IActionResult> GetCurrencyCharges()
        //{
        //    var result = await _accountService.GetCurrencyCharges();
        //    return Ok(result);
        //}


    }
}





