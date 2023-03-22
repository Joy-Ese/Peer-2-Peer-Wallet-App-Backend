using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WalletPayment.Models.DataObjects;
using WalletPayment.Services.Interfaces;

namespace WalletPayment.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly DataContext _context;
        private IEmail _emailService;

        public EmailController(DataContext context, IEmail emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpPost("ForgetPassword")]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordDto emailReq)
        {
            var result = await _emailService.ForgetPassword(emailReq);
            return Ok(result);
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordReq)
        {
            var result = await _emailService.ResetPassword(resetPasswordReq);
            return Ok(result);
        }

    }
}




