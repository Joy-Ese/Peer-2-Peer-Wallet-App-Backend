using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WalletPayment.Models.DataObjects;
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

        [HttpPost("sign-up")]
        public async Task<IActionResult> Register([FromBody] UserSignUpDto request)
        {
            var result = await _userService.Register(request);
            return Ok("Registration Successful");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto request)
        {
            var result = await _userService.Login(request);
            return Ok(result);
        }
    }
}
