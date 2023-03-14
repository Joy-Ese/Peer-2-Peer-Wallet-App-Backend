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

        //[HttpPost("SendCreditEmail")]
        //public async Task<IActionResult> SendCreditEmail(string senderEmail, string recipient, string amount, string balance, string date, string username)
        //{
        //    bool result = await _emailService.SendCreditEmail(senderEmail, recipient, amount, balance, date, username);
        //    if (!result)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, "An error occured. The Mail could not be sent.");
        //    }

        //    return StatusCode(StatusCodes.Status200OK, "Mail has successfully been sent.");
        //}

        //[HttpPost("SendDebitEmail")]
        //public async Task<IActionResult> SendDebitEmail(string recepientEmail, string sender, string amount2, string balance2, string date2, string username2)
        //{
        //    bool result = await _emailService.SendDebitEmail(recepientEmail, sender, amount2, balance2, date2, username2);
        //    if (!result)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, "An error occured. The Mail could not be sent.");
        //    }

        //    return StatusCode(StatusCodes.Status200OK, "Mail has successfully been sent.");
        //}

        //[HttpPost("SendDepositEmail")]
        //public async Task<IActionResult> SendDepositEmail(string selfEmail, string selfName, string selfAmount, string selfBalance, string date3)
        //{
        //    bool result = await _emailService.SendDepositEmail(selfEmail, selfName, selfAmount, selfBalance, date3);
        //    if (!result)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, "An error occured. The Mail could not be sent.");
        //    }

        //    return StatusCode(StatusCodes.Status200OK, "Mail has successfully been sent.");
        //}
    }
}




