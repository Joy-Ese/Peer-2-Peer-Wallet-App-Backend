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

        [HttpPost("CreateTransfer")]
        public async Task<IActionResult> TransferFund(TransactionDto request)
        {
            var result = await _transactionService.TransferFund(request);
            return Ok(result);
        }



    }
}






