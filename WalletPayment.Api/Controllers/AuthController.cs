using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WalletPayment.Models.DataObjects;
using WalletPayment.Models.Entites;
using WalletPayment.Services.Interfaces;

namespace WalletPayment.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DataContext _context;
        private IAuth _authService;

        public AuthController(DataContext context, IAuth authService)
        {
            _context = context;
            _authService = authService;
        }

        [HttpPost("CreateAdmin"), Authorize]
        public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminDTO request)
        {
            var result = await _authService.CreateAdmin(request);
            return Ok(result);
        }

        [HttpPost("SignUp")]
        public async Task<IActionResult> Register([FromBody] UserSignUpDto request)
        {
            var result = await _authService.Register(request);
            return Ok(result);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserLoginDto request)
        {
            var result = await _authService.Login(request);
            if (!result.status) return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("AdminLogin")]
        public async Task<IActionResult> AdminLogin(AdminLoginDTO request)
        {
            var result = await _authService.AdminLogin(request);
            if (!result.status) return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("CreatePin"), Authorize]
        public async Task<IActionResult> CreatePin(CreatePinDto request)
        {
            var result = await _authService.CreatePin(request);
            return Ok(result);

        }

        [HttpPut("UpdateUserPin"), Authorize]
        public async Task<IActionResult> UpdatePin(UpdatePinDto request)
        {
            var result = await _authService.UpdatePin(request);
            return Ok(result);
        }

        [HttpPost("ForgetPassword")]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordDto emailReq)
        {
            var result = await _authService.ForgetPassword(emailReq);
            return Ok(result);
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordReq)
        {
            var result = await _authService.ResetPassword(resetPasswordReq);
            return Ok(result);
        }

        [HttpPost("VerifyEmail")]
        public async Task<IActionResult> VerifyEmail(VerifyEmailDto verifyReq)
        {
            var result = await _authService.VerifyEmail(verifyReq);
            return Ok(result);
        }

        [HttpPost("ChangePassword"), Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto request)
        {
            var result = await _authService.ChangePassword(request);
            return Ok(result);
        }

        [HttpPost("RefreshToken"), Authorize]
        public async Task<IActionResult> RefreshToken()
        {
            var result = await _authService.RefreshToken();
            return Ok(result);
        }
    }
}
