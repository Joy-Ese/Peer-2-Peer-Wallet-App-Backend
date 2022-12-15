﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WalletPayment.Models.DataObjects;
using WalletPayment.Services.Interfaces;

namespace WalletPayment.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly DataContext _context;
        private ITransaction _transactionService;
        private IUser _userService;

        public TransactionController(DataContext context, ITransaction transactionService, IUser userService)
        {
            _context = context;
            _transactionService = transactionService;
            _userService = userService;
        }

        [HttpPost("CreateTransfer"), Authorize]
        public async Task<IActionResult> TransferFund(TransactionDto request)
        {
            var result = await _transactionService.TransferFund(request);
            return Ok(result);
        }

        [HttpGet("TransactionDetails"), Authorize]
        public async Task<IActionResult> GetTransactionDetails(string AccountNumber)
        {
            var result = await _transactionService.GetTransactionDetails(AccountNumber);
            return Ok(result);
        }

    }
}






