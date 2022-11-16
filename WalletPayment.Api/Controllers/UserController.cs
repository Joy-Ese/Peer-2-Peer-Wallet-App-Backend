﻿using Microsoft.AspNetCore.Http;
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
            return Ok(result);
        }

        [HttpPost("Authenticate")]
        public async Task<IActionResult> Authenticate(string AccountNumber)
        {
            var result = await _userService.Authenticate(AccountNumber);
            return Ok(result);
        }
    }
}
