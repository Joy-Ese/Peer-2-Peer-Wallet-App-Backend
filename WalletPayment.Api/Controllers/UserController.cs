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
    public class UserController : ControllerBase
    {
        private readonly DataContext _context;
        private IUser _userService;

        public UserController(DataContext context, IUser userService)
        {
            _context = context;
            _userService = userService;
        }

        [HttpPost("SignUp")]
        public async Task<IActionResult> Register([FromBody] UserSignUpDto request)
        {
            var result = await _userService.Register(request);
            return Ok("Registration Successful");
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserLoginDto request)
        {
            var result = await _userService.Login(request);
            if (!result.status) return BadRequest(result);

            return Ok(result);

        }

        [HttpPost("RefreshToken"), Authorize]
        public async Task<IActionResult> RefreshToken()
        {
            var result = await _userService.RefreshToken();
            return Ok(result);
        }
    }
}
