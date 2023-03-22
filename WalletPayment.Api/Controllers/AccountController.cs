using Microsoft.AspNetCore.Authorization;
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

        [HttpPost("AccountLookUp"), Authorize]
        public async Task<IActionResult> AccountLookUp(string searchInfo)
        {
            var result = await _accountService.AccountLookUp(searchInfo);
            return Ok(result);
        }
    }
}





