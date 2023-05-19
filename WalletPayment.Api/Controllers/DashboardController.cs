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

        [HttpGet("GetUserDetails"), Authorize]
        public async Task<IActionResult> GetUserDetails()
        {
            var result = await _dashboardService.GetUserDetails();
            return Ok(result);
        }

        [HttpGet("GetUserAccountBalance"), Authorize]
        public async Task<IActionResult> GetUserAccountBalance()
        {
            var result = await _dashboardService.GetUserAccountBalance();
            return Ok(result);
        }

        [HttpGet("GetUserAccountNumber"), Authorize]
        public async Task<IActionResult> GetUserAccountNumber()
        {
            var result = await _dashboardService.GetUserAccountNumber();
            return Ok(result);
        }

        [HttpGet("GetUserAccountCurrency"), Authorize]
        public async Task<IActionResult> GetUserAccountCurrency()
        {
            var result = await _dashboardService.GetUserAccountCurrency();
            return Ok(result);
        }

        [HttpGet("GetUserEmail"), Authorize]
        public async Task<IActionResult> GetUserEmail()
        {
            var result = await _dashboardService.GetUserEmail();
            return Ok(result);
        }
        
        [HttpGet("GetUserProfile"), Authorize]
        public async Task<IActionResult> GetUserProfile()
        {
            var result = await _dashboardService.GetUserProfile();
            return Ok(result);
        }
        
        [HttpPut("UpdateUserInfo"), Authorize]
        public async Task<IActionResult> UpdateUserInfo(UpdateUserInfoDto request)
        {
            var result = await _dashboardService.UpdateUserInfo(request);
            return Ok(result);
        }
        
        [HttpPost("UploadNewImage"), Authorize]
        public async Task<IActionResult> UploadNewImage([FromForm]ImageRequestDTO req)
        {
            var result = await _dashboardService.UploadNewImage(req.ImageDetails);
            return Ok(result);
        }

        [HttpPut("UpdateImage"), Authorize]
        public async Task<IActionResult> UpdateImage([FromForm] ImageRequestDTO req)
        {
            var result = await _dashboardService.UpdateImage(req.ImageDetails);
            return Ok(result);
        }

        [HttpGet("GetUserImage"), Authorize]
        public async Task<IActionResult> GetUserImage()
        {
            var result = await _dashboardService.GetUserImage();
            return Ok(result);
        }

        [HttpDelete("DeleteUserImage"), Authorize]
        public async Task<IActionResult> DeleteUserImage()
        {
            var result = await _dashboardService.DeleteUserImage();
            return Ok(result);
        }

        [HttpPost("SetSecurityQuestion"), Authorize]
        public async Task<IActionResult> SetSecurityQuestion(SecurityQuestionDto request)
        {
            var result = await _dashboardService.SetSecurityQuestion(request);
            return Ok(result);
        }

        [HttpGet("GetUserSecurityQuestion"), Authorize]
        public async Task<IActionResult> GetUserSecurityQuestion()
        {
            var result = await _dashboardService.GetUserSecurityQuestion();
            return Ok(result);
        }

        [HttpGet("GetUserSecurityAnswer"), Authorize]
        public async Task<IActionResult> GetUserSecurityAnswer()
        {
            var result = await _dashboardService.GetUserSecurityAnswer();
            return Ok(result);
        }

        [HttpGet("GetUserPin"), Authorize]
        public async Task<IActionResult> GetUserPin()
        {
            var result = await _dashboardService.GetUserPin();
            return Ok(result);
        }

    }
}
