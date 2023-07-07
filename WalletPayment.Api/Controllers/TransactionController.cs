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
        private ITransaction _transactionService;

        public TransactionController(ITransaction transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpPost("CreateTransfer"), Authorize]
        public async Task<IActionResult> TransferFund(TransactionDto request)
        {
            var result = await _transactionService.TransferFund(request);
            //await _notificationHubContext.Clients.Clients;
            return Ok(result);
        }

        [HttpGet("GetTransactionList"), Authorize]
        public async Task<IActionResult> GetTransactionList()
        {
            var result = await _transactionService.GetTransactionList();
            return Ok(result);
        }

        [HttpGet("GetLastThreeTransactions"), Authorize]
        public async Task<IActionResult> GetLastThreeTransactions()
        {
            var result = await _transactionService.GetLastThreeTransactions();
            return Ok(result);
        }

        [HttpPost("TransactionsByDateRange"), Authorize]
        public async Task<IActionResult> TransactionsByDateRange(TransactionDateDto request)
        {
            var result = await _transactionService.TransactionsByDateRange(request);
            return Ok(result);
        }

        [HttpPost("GeneratePDFStatement"), Authorize]
        public async Task<IActionResult> GeneratePDFStatement(CreateStatementRequestDTO request)
        {
            var result = await _transactionService.GeneratePDFStatement(request);
            return Ok(result);
        }

        [HttpPost("GenerateExcelStatement"), Authorize]
        public async Task<IActionResult> GenerateExcelStatement(CreateStatementRequestDTO request)
        {
            var result = await _transactionService.GenerateExcelStatement(request);
            return Ok(result);
        }

    }
}






