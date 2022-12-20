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

        [HttpPost("SignUp")]
        public async Task<IActionResult> Register([FromBody] UserSignUpDto request)
        {
            var result = await _authService.Register(request);
            return Ok("Registration Successful");
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserLoginDto request)
        {
            var result = await _authService.Login(request);
            if (!result.status) return BadRequest(result);

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
