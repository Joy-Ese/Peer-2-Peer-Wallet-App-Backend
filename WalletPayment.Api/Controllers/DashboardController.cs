using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WalletPayment.Services.Interfaces;
using Microsoft.Extensions.Logging;

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
    }
}
