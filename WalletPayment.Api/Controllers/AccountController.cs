using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WalletPayment.Models.Entites;
using WalletPayment.Services.Interfaces;

namespace WalletPayment.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly DataContext _context;
        private IAccount _accountService;

        public AccountController(DataContext context, IAccount accountService)
        {
            _context = context;
            _accountService = accountService;
        }

        [HttpGet("GetByAccountNumber")]
        public async Task<IActionResult> GetByAccountNumber(string AccountNumber)
        {
            var result = await _accountService.GetByAccountNumber(AccountNumber);
            return Ok(result);
        }


}
}





