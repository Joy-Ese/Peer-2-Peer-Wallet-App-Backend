using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WalletPayment.Models.DataObjects;
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

        [HttpPut("UpdateChargeOrRate"), Authorize]
        public async Task<IActionResult> UpdateChargeOrRate(UpdateChargeOrRateDTO req)
        {
            var result = await _accountService.UpdateChargeOrRate(req);
            return Ok(result);
        }

        [HttpPost("CreateSystemAccount"), Authorize]
        public async Task<IActionResult> CreateSystemAccount(CreateSystemAccountsDTO request)
        {
            var result = await _accountService.CreateSystemAccount(request);
            return Ok(result);
        }

        [HttpPost("AccountLookUp"), Authorize]
        public async Task<IActionResult> AccountLookUp(string searchInfo, string currency)
        {
            var result = await _accountService.AccountLookUp(searchInfo, currency);
            return Ok(result);
        }

        [HttpPost("SendMoneyCheck"), Authorize]
        public async Task<IActionResult> SendMoneyCheck(string currency)
        {
            var result = await _accountService.SendMoneyCheck(currency);
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

        [HttpGet("CurrenciesSeededInDb"), Authorize]
        public async Task<IActionResult> CurrenciesSeededInDb()
        {
            var result = await _accountService.CurrenciesSeededInDb();
            return Ok(result);
        }

        [HttpGet("FundWalletCurrencies"), Authorize]
        public async Task<IActionResult> FundWalletCurrencies()
        {
            var result = await _accountService.FundWalletCurrencies();
            return Ok(result);
        }

        [HttpGet("UserAccountDetails"), Authorize]
        public async Task<IActionResult> UserAccountDetails()
        {
            var result = await _accountService.UserAccountDetails();
            return Ok(result);
        }

        [HttpGet("GetCurrencyCharges"), Authorize]
        public async Task<IActionResult> GetCurrencyCharges()
        {
            var result = await _accountService.GetCurrencyCharges();
            return Ok(result);
        }

        [HttpGet("GetConversionRates"), Authorize]
        public async Task<IActionResult> GetConversionRates()
        {
            var result = await _accountService.GetConversionRates();
            return Ok(result);
        }

        [HttpPost("FundForeignWallet"), Authorize]
        public async Task<IActionResult> FundForeignWallet(FundWalletDTO req)
        {
            var result = await _accountService.FundForeignWallet(req);
            return Ok(result);
        }

        [HttpGet("GetNairaBalance"), Authorize]
        public async Task<IActionResult> GetNairaBalance()
        {
            var result = await _accountService.GetNairaBalance();
            return Ok(result);
        }

        [HttpGet("GetUserDataForAdmin"), Authorize]
        public async Task<IActionResult> GetUserDataForAdmin()
        {
            var result = await _accountService.GetUserDataForAdmin();
            return Ok(result);
        }

        [HttpGet("SystemAccountDetails"), Authorize]
        public async Task<IActionResult> SystemAccountDetails()
        {
            var result = await _accountService.SystemAccountDetails();
            return Ok(result);
        }

        [HttpPut("LockOrUnlockUsers"), Authorize]
        public async Task<IActionResult> LockOrUnlockUsers(LockOrUnlockUsersDTO req)
        {
            var result = await _accountService.LockOrUnlockUsers(req);
            return Ok(result);
        }

        [HttpGet("GetLockedUsersList"), Authorize]
        public async Task<IActionResult> GetLockedUsersList()
        {
            var result = await _accountService.GetLockedUsersList();
            return Ok(result);
        }


    }
}





