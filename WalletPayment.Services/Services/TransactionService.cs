using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletPayment.Models.DataObjects;
using WalletPayment.Models.Entites;
using WalletPayment.Services.Data;
using WalletPayment.Services.Interfaces;

namespace WalletPayment.Services.Services
{
    public class TransactionService : ITransaction
    {
        private readonly DataContext _context;
        private readonly IUser _userService;
        private readonly IAccount _accountService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<TransactionService> _logger;


        public TransactionService(DataContext context, IUser userService, IAccount accountService,
            IHttpContextAccessor httpContextAccessor, ILogger<TransactionService> logger)
        {
            _context = context;
            _userService = userService;
            _accountService = accountService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _logger.LogDebug(1, "Nlog injected into TransactionService");
        }

        public async Task<Responses> FindTransactionByDate(DateTime date)
        {
            Responses response = new Responses();
            var transaction = _context.Transactions.Where(tran => tran.TransactionDate == date).ToList();

            response.ResponseCode = "00";
            response.ResponseMessage = "Transaction found successfully!";
            response.Data = transaction;

            return response;
        }

        public async Task<Responses> TransferFund(TransactionDto request)
        {
            Responses response = new Responses();
            Transaction transaction = new Transaction();

            try
            {
                if (request.amount <= 0)
                {
                    throw new ApplicationException("Transfer amount cannot be zero or negative");
                }

                var sourceAccountData = await _accountService.GetByAccountNumber(request.sourceAccount);
                var destinationAccountData = await _accountService.GetByAccountNumber(request.destinationAccount);
                if (sourceAccountData == null || destinationAccountData == null)
                    throw new ApplicationException("Invalid source or desitnation account");

                if (sourceAccountData.Balance < request.amount)
                {
                    throw new ApplicationException("Insufficient Funds");
                }

                using var dbTransaction = _context.Database.BeginTransaction();

                sourceAccountData.Balance -= request.amount;
                destinationAccountData.Balance += request.amount;

                var result = await _context.SaveChangesAsync();
                if (!(result > 0))
                {
                    throw new ApplicationException("Transaction failed");
                }

                transaction.TranSourceAccount = request.sourceAccount;
                transaction.TranDestinationAccount = request.destinationAccount;
                transaction.TransactionAmount = request.amount;
                transaction.TransactionDate = DateTime.Now;

                await _context.Transactions.AddAsync(transaction);
                await _context.SaveChangesAsync();

                dbTransaction.Commit();

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                _logger.LogInformation("The error occurred at",
                    DateTime.UtcNow.ToLongTimeString());
                return response;

            }
        }

        public async Task<List<TransactionViewModel>> GetTransactionDetails(string AccountNumber)
        {
            List<TransactionViewModel> transactionsList = new List<TransactionViewModel>();
            try
            {
                var data = await _context.Transactions.Where(a => a.TranSourceAccount == AccountNumber).ToListAsync();
                data.ForEach(t => transactionsList.Add(new TransactionViewModel
                {
                    amount = t.TransactionAmount,
                    sourceAccount = t.TranSourceAccount,
                    destinationAccount = t.TranDestinationAccount,
                    date = t.TransactionDate,
                }));

                return transactionsList;

            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                _logger.LogInformation("The error occurred at",
                    DateTime.UtcNow.ToLongTimeString());
                return new List<TransactionViewModel>();
            }
        }

    }
}














