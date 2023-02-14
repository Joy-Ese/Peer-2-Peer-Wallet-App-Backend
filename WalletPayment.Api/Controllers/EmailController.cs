﻿using Microsoft.AspNetCore.Authorization;
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

        [HttpPost("SendEmail"), Authorize]
        public async Task<IActionResult> SendEmail(EmailDto request)
        {
            bool result = await _emailService.SendEmail(request, request.to);
            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occured. The Mail could not be sent.");
            }

            return StatusCode(StatusCodes.Status200OK, "Mail has successfully been sent.");
        }

        [HttpPost("SendCreditEmail"), Authorize]
        public async Task<IActionResult> SendCreditEmail(EmailDto request)
        {
            bool result = await _emailService.SendCreditEmail(request, request.to);
            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occured. The Mail could not be sent.");
            }

            return StatusCode(StatusCodes.Status200OK, "Mail has successfully been sent.");
        }

        [HttpPost("SendDebitEmail"), Authorize]
        public async Task<IActionResult> SendDebitEmail(EmailDto request)
        {
            bool result = await _emailService.SendDebitEmail(request, request.to);
            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occured. The Mail could not be sent.");
            }

            return StatusCode(StatusCodes.Status200OK, "Mail has successfully been sent.");
        }
    }
}




