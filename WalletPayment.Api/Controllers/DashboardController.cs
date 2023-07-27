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

        [HttpGet("DoesUserHaveImage"), Authorize]
        public async Task<IActionResult> DoesUserHaveImage()
        {
            var result = await _dashboardService.DoesUserHaveImage();
            return Ok(result);
        }

        [HttpGet("NoSecurityAttemptsLeft"), Authorize]
        public async Task<IActionResult> NoSecurityAttemptsLeft()
        {
            var result = await _dashboardService.NoSecurityAttemptsLeft();
            return Ok(result);
        }

        [HttpPost("KycUpload"), Authorize]
        public async Task<IActionResult> KycUpload([FromForm] ImageRequestDTO req)
        {
            var result = await _dashboardService.KycUpload(req.ImageDetails);
            return Ok(result);
        }

        [HttpPut("KycReUpload"), Authorize]
        public async Task<IActionResult> KycReUpload([FromForm] ImageRequestDTO req)
        {
            var result = await _dashboardService.KycReUpload(req.ImageDetails);
            return Ok(result);
        }

        [HttpGet("GetKycUploadsForAdmin"), Authorize]
        public async Task<IActionResult> GetKycUploadsForAdmin()
        {
            var result = await _dashboardService.GetKycUploadsForAdmin();
            return Ok(result);
        }

        [HttpGet("GetUserInfoOnKycUploadsForAdmin"), Authorize]
        public async Task<IActionResult> GetUserInfoOnKycUploadsForAdmin()
        {
            var result = await _dashboardService.GetUserInfoOnKycUploadsForAdmin();
            return Ok(result);
        }

        [HttpPut("RemoveImage"), Authorize]
        public async Task<IActionResult> RemoveImage(string filename, string userId)
        {
            var result = await _dashboardService.RemoveImage(filename, userId);
            return Ok(result);
        }

        [HttpPut("AcceptImage"), Authorize]
        public async Task<IActionResult> AcceptImage(string filename, string userId)
        {
            var result = await _dashboardService.AcceptImage(filename, userId);
            return Ok(result);
        }

        [HttpGet("GetUserProfileLevel"), Authorize]
        public async Task<IActionResult> GetUserProfileLevel()
        {
            var result = await _dashboardService.GetUserProfileLevel();
            return Ok(result);
        }

        [HttpGet("AllAdminsLists"), Authorize]
        public async Task<IActionResult> AllAdminsLists()
        {
            var result = await _dashboardService.AllAdminsLists();
            return Ok(result);
        }

        [HttpPut("DisableEnableAdmin"), Authorize]
        public async Task<IActionResult> DisableEnableAdmin(DisableEnableAdminDTO req)
        {
            var result = await _dashboardService.DisableEnableAdmin(req);
            return Ok(result);
        }

        [HttpGet("GetKycStatus"), Authorize]
        public async Task<IActionResult> GetKycStatus()
        {
            var result = await _dashboardService.GetKycStatus();
            return Ok(result);
        }



    }
}
