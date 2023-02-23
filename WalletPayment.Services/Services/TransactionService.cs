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
        private readonly IAccount _accountService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<TransactionService> _logger;


        public TransactionService(DataContext context, IAccount accountService,
            IHttpContextAccessor httpContextAccessor, ILogger<TransactionService> logger)
        {
            _context = context;
            _accountService = accountService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _logger.LogDebug(1, "Nlog injected into TransactionService");
        }

        public async Task<TransactionResponseModel> TransferFund(TransactionDto request)
        {
            TransactionResponseModel transactionResponse = new TransactionResponseModel();
            Transaction transaction = new Transaction();

            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return transactionResponse;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var data = await _context.Users
                    .Where(uProfile => uProfile.Id == userID)
                    .FirstOrDefaultAsync();

                if (!AuthService.VerifyPinHash(request.pin, data.PinHash, data.PinSalt))
                {
                    transactionResponse.responseMessage = "Invalid Pin";
                    return transactionResponse;
                }

                if (request.amount <= 0)
                {
                    transactionResponse.responseMessage = "Transfer amount cannot be zero or negative";
                    return transactionResponse;
                }

                var sourceAccountData = await _accountService.GetByAccountNumber(request.sourceAccount);
                var destinationAccountData = await _accountService.GetByAccountNumber(request.destinationAccount);

                if (sourceAccountData == null || destinationAccountData == null)
                    throw new ApplicationException("Invalid source or desitnation account");

                if (sourceAccountData.Balance < request.amount)
                {
                    transactionResponse.responseMessage = "Insufficient Funds";
                    return transactionResponse;
                }

                using var dbTransaction = _context.Database.BeginTransaction();

                sourceAccountData.Balance -= request.amount;
                destinationAccountData.Balance += request.amount;

                var result = await _context.SaveChangesAsync();
                if (!(result > 0))
                {
                    transactionResponse.responseMessage = "Transaction failed";
                    transaction.Status = StatusMessage.Failed.ToString();
                    return transactionResponse;
                }

                transaction.TranSourceAccount = request.sourceAccount;
                transaction.TranDestinationAccount = request.destinationAccount;
                transaction.Amount = request.amount;
                transaction.Date = DateTime.Now;
                transaction.UserId = data.Id;
                transaction.Status = StatusMessage.Successful.ToString();

                await _context.Transactions.AddAsync(transaction);
                await _context.SaveChangesAsync();

                dbTransaction.Commit();

                //var senderEmail = sourceAccountData.User.Email;
                //var recepientEmail = destinationAccountData.User.Email;

                transactionResponse.status = true;
                transactionResponse.responseMessage = "Transaction successful";
                return transactionResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                transactionResponse.responseMessage = "An exception occured";
                return transactionResponse;

            }
        }

        public async Task<List<TransactionViewModel>> GetTransactionDetails()
        {
            List<TransactionViewModel> transactionsList = new List<TransactionViewModel>();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return transactionsList;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var userTranList = await _context.Transactions
                                    .Where(uId => uId.UserId == userID).ToListAsync();
                        userTranList.ForEach(tranList => transactionsList.Add(new TransactionViewModel
                        {
                            amount = tranList.Amount,
                            sourceAccount = tranList.TranSourceAccount,
                            destinationAccount = tranList.TranDestinationAccount,
                            date = tranList.Date
                        }));

                return transactionsList;

            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                _logger.LogInformation("The error occurred at",
                    DateTime.UtcNow.ToLongTimeString());
                return transactionsList;
            }
        }

        public async Task<List<TransactionCreditModel>> GetTransactionCreditDetails()
        {
            List<TransactionCreditModel> transactionsCreditList = new List<TransactionCreditModel>();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return transactionsCreditList;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                //var userLoggedInAcct = await _context.Users.Where(getID => getID.Id == userID).FirstOrDefaultAsync();

                var userCreditTranList = await _context.Transactions
                            .Where(uId => uId.Id == userID)
                            .Select(uId => new TransactionCreditModel
                            {
                                amount = uId.Amount,
                                sender = uId.User.Username,
                                date = uId.Date
                            })
                            .ToListAsync();

                        //userCreditTranList.ForEach(tranList => transactionsCreditList.Add(new TransactionCreditModel
                        //{
                        //    amount = tranList.Amount,
                        //    sender = tranList.User.Username,
                        //    date = tranList.Date
                        //}));

                                //await _context.Users
                                //.Where(userInfo => userInfo.Id == userID)
                                //.Select(userInfo => new UserDashboardViewModel
                                //{
                                //    Username = userInfo.Username,
                                //    FirstName = userInfo.FirstName,
                                //    LastName = userInfo.LastName,
                                //    AccountNumber = userInfo.UserAccount.AccountNumber,
                                //    Balance = userInfo.UserAccount.Balance.ToString(),
                                //})
                                //.FirstOrDefaultAsync();

                return transactionsCreditList;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                _logger.LogInformation("The error occurred at",
                    DateTime.UtcNow.ToLongTimeString());
                return transactionsCreditList;
            }
        }
    }
}














