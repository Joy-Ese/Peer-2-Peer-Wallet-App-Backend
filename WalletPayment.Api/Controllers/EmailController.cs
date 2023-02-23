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

        [HttpPost("SendEmail")]
        public async Task<IActionResult> SendEmail(EmailDto request)
        {
            bool result = await _emailService.SendEmail(request, request.to);
            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occured. The Mail could not be sent.");
            }

            return StatusCode(StatusCodes.Status200OK, "Mail has successfully been sent.");
        }

        [HttpPost("SendEmailPasswordReset")]
        public async Task<IActionResult> SendEmailPasswordReset(string Link, string emailUser)
        {
            bool result = await _emailService.SendEmailPasswordReset(Link, emailUser);
            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occured. The Mail could not be sent.");
            }

            return StatusCode(StatusCodes.Status200OK, "Mail has successfully been sent.");
        }

        [HttpPost("ForgetPassword")]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordDto emailReq)
        {
            var result = await _emailService.ForgetPassword(emailReq);
            return Ok(result);
        }

        [HttpGet("GetResetPassword")]
        public async Task<IActionResult> GetResetPassword(string token, string email)
        {
            var result = await _emailService.GetResetPassword(token, email);
            return Ok(result);
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordReq)
        {
            var result = await _emailService.ResetPassword(resetPasswordReq);
            return Ok(result);
        }

        [HttpPost("SendCreditEmail")]
        public async Task<IActionResult> SendCreditEmail(string senderEmail)
        {
            bool result = await _emailService.SendCreditEmail(senderEmail);
            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occured. The Mail could not be sent.");
            }

            return StatusCode(StatusCodes.Status200OK, "Mail has successfully been sent.");
        }

        [HttpPost("SendDebitEmail")]
        public async Task<IActionResult> SendDebitEmail(string recepientEmail)
        {
            bool result = await _emailService.SendDebitEmail(recepientEmail);
            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occured. The Mail could not be sent.");
            }

            return StatusCode(StatusCodes.Status200OK, "Mail has successfully been sent.");
        }
    }
}




