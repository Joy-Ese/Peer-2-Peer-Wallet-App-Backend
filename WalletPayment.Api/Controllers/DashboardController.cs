using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WalletPayment.Services.Interfaces;
using Microsoft.Extensions.Logging;
using WalletPayment.Models.DataObjects;

namespace WalletPayment.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly DataContext _context;
        private IDashboard _dashboardService;

        public DashboardController(DataContext context, IDashboard dashboardService)
        {
            _context = context;
            _dashboardService = dashboardService;
        }

        [HttpGet("UserDetails"), Authorize]
        public async Task<IActionResult> GetUserDetails()
        {
            var result = await _dashboardService.GetUserDetails();
            return Ok(result);
        }

        [HttpGet("AccountBalance"), Authorize]
        public async Task<IActionResult> GetUserAccountBalance()
        {
            var result = await _dashboardService.GetUserAccountBalance();
            return Ok(result);
        }

        [HttpGet("UserAccountNumber"), Authorize]
        public async Task<IActionResult> GetUserAccountNumber()
        {
            var result = await _dashboardService.GetUserAccountNumber();
            return Ok(result);
        }

        [HttpPut("UserProfile"), Authorize]
        public async Task<IActionResult> UpdateUserProfile([FromBody]UserUpdateDto request)
        {
            var result = await _dashboardService.UpdateUserProfile(request);
            if (!result.status) return BadRequest(result);

            return Ok(result);
        }
    }
}
