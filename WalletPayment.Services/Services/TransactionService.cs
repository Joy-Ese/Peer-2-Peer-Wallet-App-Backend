﻿using Microsoft.AspNetCore.Http;
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
        private readonly IEmail _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<TransactionService> _logger;


        public TransactionService(DataContext context, IEmail emailService, IAccount accountService,
            IHttpContextAccessor httpContextAccessor, ILogger<TransactionService> logger)
        {
            _context = context;
            _accountService = accountService;
            _emailService = emailService;
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

                var userLoggedInDetails = await _context.Users
                    .Where(uProfile => uProfile.Id == userID)
                    .FirstOrDefaultAsync();

                if (!AuthService.VerifyPinHash(request.pin, userLoggedInDetails.PinHash, userLoggedInDetails.PinSalt))
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
                {
                    transactionResponse.responseMessage = "Account Number does not exist";
                    return transactionResponse;
                }

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
                transaction.SourceAccountUserId = userLoggedInDetails.Id;
                transaction.DestinationAccountUserId = destinationAccountData.User.Id;
                transaction.Status = StatusMessage.Successful.ToString();

                await _context.Transactions.AddAsync(transaction);
                await _context.SaveChangesAsync();

                // Debit Email information
                var senderEmail = sourceAccountData.User.Email;
                var sender = sourceAccountData.User.FirstName;
                var amount2 = transaction.Amount.ToString();
                var balance2 = sourceAccountData.Balance.ToString();
                var date2 = transaction.Date.ToLongDateString();
                var username2 = destinationAccountData.User.Username;

                // Credit Email information
                var recepientEmail = destinationAccountData.User.Email;
                var recipient = destinationAccountData.User.FirstName;
                var amount = transaction.Amount.ToString();
                var balance = destinationAccountData.Balance.ToString();
                var date = transaction.Date.ToLongDateString();
                var username = sourceAccountData.User.Username;

                await _emailService.SendDebitEmail(senderEmail, sender, amount2, balance2, date2, username2);
                await _emailService.SendCreditEmail(recepientEmail, recipient, amount, balance, date, username);

                dbTransaction.Commit();

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

        //public async Task<List<TransactionViewModel>> GetTransactionDetails()
        //{
        //    List<TransactionViewModel> transactionsList = new List<TransactionViewModel>();
        //    try
        //    {
        //        int userID;
        //        if (_httpContextAccessor.HttpContext == null)
        //        {
        //            return transactionsList;
        //        }

        //        userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

        //        var userTran = await _context.Transactions.Include("User")
        //                            .Where(uId => uId.UserId == userID).FirstOrDefaultAsync();

        //        var userTranList = await _context.Transactions.Include("User")
        //                            .Where(uId => uId.UserId == userID).ToListAsync();

        //        //var acctNum = await _context.Accounts.Include("User").Where(acc => acc.UserId == userID).FirstOrDefaultAsync();

        //        var getRecepient = await _context.Accounts.Include("User")
        //                            .Where(gR => gR.AccountNumber == userTran.TranDestinationAccount).FirstOrDefaultAsync();

        //        userTranList.ForEach(tranList => transactionsList.Add(new TransactionViewModel
        //        {
        //            amount = tranList.Amount,
        //            sourceAccount = tranList.TranSourceAccount,
        //            destinationAccount = tranList.TranDestinationAccount,
        //            recepient = getRecepient.User.Username,
        //            date = tranList.Date,
        //            status = tranList.Status
        //        }));

        //        return transactionsList;

        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
        //        _logger.LogInformation("The error occurred at",
        //            DateTime.UtcNow.ToLongTimeString());
        //        return transactionsList;
        //    }
        //}

        public async Task<List<TransactionListCreditModel>> GetTransactionList()
        {
            List<TransactionListCreditModel> transactionsCreditList = new List<TransactionListCreditModel>();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return transactionsCreditList;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var loggedInUser = await _context.Accounts.Include("User").Where(senderID => senderID.Id == userID).FirstOrDefaultAsync();

                var userTranList = await _context.Transactions.Include("SourceUser").Include("DestinationUser")
                            .Where(txn => txn.TranDestinationAccount == loggedInUser.AccountNumber || txn.TranSourceAccount == loggedInUser.AccountNumber).ToListAsync();


                userTranList.ForEach(tranCreditList => transactionsCreditList.Add(new TransactionListCreditModel
                {
                    amount = tranCreditList.Amount,
                    sender = tranCreditList.SourceUser.Username,
                    senderAccount = tranCreditList.TranSourceAccount,
                    recepient = tranCreditList.DestinationUser.Username,
                    recepientAccount = tranCreditList.TranDestinationAccount,
                    status = tranCreditList.Status,
                    date = tranCreditList.Date
                }));

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














