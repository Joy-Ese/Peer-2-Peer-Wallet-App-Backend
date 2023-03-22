using Microsoft.AspNetCore.Authorization;
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
        private IAuth _authService;

        public TransactionController(DataContext context, ITransaction transactionService, IAuth authService)
        {
            _context = context;
            _transactionService = transactionService;
            _authService = authService;
        }

        [HttpPost("CreateTransfer"), Authorize]
        public async Task<IActionResult> TransferFund(TransactionDto request)
        {
            var result = await _transactionService.TransferFund(request);
            return Ok(result);
        }

        [HttpGet("GetTransactionList"), Authorize]
        public async Task<IActionResult> GetTransactionList()
        {
            var result = await _transactionService.GetTransactionList();
            return Ok(result);
        }

    }
}






