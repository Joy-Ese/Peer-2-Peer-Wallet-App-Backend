using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        private IConverter _converter;
        private readonly IAccount _accountService;
        private readonly IEmail _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<TransactionService> _logger;


        public TransactionService(DataContext context, IEmail emailService, IAccount accountService,
            IHttpContextAccessor httpContextAccessor, ILogger<TransactionService> logger, IConverter converter)
        {
            _context = context;
            _converter = converter;
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

        public async Task<List<TransactionListModel>> GetTransactionList()
        {
            List<TransactionListModel> transactionsCreditList = new List<TransactionListModel>();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return transactionsCreditList;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var txnType = "";

                var loggedInUser = await _context.Accounts.Include("User").Where(senderID => senderID.UserId == userID).FirstOrDefaultAsync();

                var userTranList = await _context.Transactions.Include("SourceUser").Include("DestinationUser")
                            .Where(txn => txn.TranDestinationAccount == loggedInUser.AccountNumber 
                            || txn.TranSourceAccount == loggedInUser.AccountNumber).ToListAsync();

                foreach (var txn in userTranList)
                {
                    if (txn.SourceAccountUserId == null && txn.TranDestinationAccount == loggedInUser.AccountNumber)
                    {
                        transactionsCreditList.Add(new TransactionListModel
                        {
                            amount = txn.Amount,
                            senderInfo = "P2P Wallet",
                            recepientInfo = $"{txn.DestinationUser.FirstName}-{txn.TranDestinationAccount}",
                            transactionType = "CREDIT",
                            currency = "NGN",
                            status = txn.Status,
                            date = txn.Date,
                        });
                    }

                    if (txn.SourceAccountUserId != null && txn.TranDestinationAccount == loggedInUser.AccountNumber)
                    {
                        transactionsCreditList.Add(new TransactionListModel
                        {
                            amount = txn.Amount,
                            senderInfo = $"{txn.SourceUser.FirstName}-{txn.TranSourceAccount}",
                            recepientInfo = $"{txn.DestinationUser.FirstName}-{txn.TranDestinationAccount}",
                            transactionType = "CREDIT",
                            currency = "NGN",
                            status = txn.Status,
                            date = txn.Date,
                        });
                    }

                    if (txn.TranSourceAccount == loggedInUser.AccountNumber)
                    {
                        transactionsCreditList.Add(new TransactionListModel
                        {
                            amount = txn.Amount,
                            senderInfo = $"{txn.SourceUser.FirstName}-{txn.TranSourceAccount}",
                            recepientInfo = $"{txn.DestinationUser.FirstName}-{txn.TranDestinationAccount}",
                            transactionType = "DEBIT",
                            currency = "NGN",
                            status = txn.Status,
                            date = txn.Date,
                        });
                    }

                }

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

        public async Task<List<TransactionListModel>> GetLastThreeTransactions()
        {
            List<TransactionListModel> getLastThreeTxns = new List<TransactionListModel>();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return getLastThreeTxns;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var txnType = "";

                var loggedInUser = await _context.Accounts.Include("User").Where(senderID => senderID.UserId == userID).FirstOrDefaultAsync();

                var last3Txns = await _context.Transactions.Include("SourceUser").Include("DestinationUser")
                                .Where(txn => txn.TranDestinationAccount == loggedInUser.AccountNumber
                                || txn.TranSourceAccount == loggedInUser.AccountNumber)
                                .OrderByDescending(txn => txn.Id).Take(3).ToListAsync();


                foreach (var txn in last3Txns)
                {
                    if (txn.SourceAccountUserId == null && txn.TranDestinationAccount == loggedInUser.AccountNumber)
                    {
                        getLastThreeTxns.Add(new TransactionListModel
                        {
                            amount = txn.Amount,
                            senderInfo = "P2P Wallet",
                            recepientInfo = $"{txn.DestinationUser.FirstName}-{txn.TranDestinationAccount}",
                            transactionType = "CREDIT",
                            currency = "NGN",
                            status = txn.Status,
                            date = txn.Date,
                        });
                    }

                    if (txn.SourceAccountUserId != null && txn.TranDestinationAccount == loggedInUser.AccountNumber)
                    {
                        getLastThreeTxns.Add(new TransactionListModel
                        {
                            amount = txn.Amount,
                            senderInfo = $"{txn.SourceUser.FirstName}-{txn.TranSourceAccount}",
                            recepientInfo = $"{txn.DestinationUser.FirstName}-{txn.TranDestinationAccount}",
                            transactionType = "CREDIT",
                            currency = "NGN",
                            status = txn.Status,
                            date = txn.Date,
                        });
                    }

                    if (txn.TranSourceAccount == loggedInUser.AccountNumber)
                    {
                        getLastThreeTxns.Add(new TransactionListModel
                        {
                            amount = txn.Amount,
                            senderInfo = $"{txn.SourceUser.FirstName}-{txn.TranSourceAccount}",
                            recepientInfo = $"{txn.DestinationUser.FirstName}-{txn.TranDestinationAccount}",
                            transactionType = "DEBIT",
                            currency = "NGN",
                            status = txn.Status,
                            date = txn.Date,
                        });
                    }

                }

                return getLastThreeTxns;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                _logger.LogInformation("The error occurred at", DateTime.UtcNow.ToLongTimeString());
                return getLastThreeTxns;
            }
        }

        public async Task<List<TransactionListModel>> TransactionsByDateRange(TransactionDateDto request)
        {
            List<TransactionListModel> txnsByDate = new List<TransactionListModel>();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return txnsByDate;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var txnType = "";

                var loggedInUser = await _context.Accounts.Include("User").Where(senderID => senderID.UserId == userID).FirstOrDefaultAsync();

                var startDate = Convert.ToDateTime(request.startDate);
                var endDate = Convert.ToDateTime(request.endDate);

                endDate = endDate.AddDays(1);

                var txnsRange = await _context.Transactions.Include("SourceUser").Include("DestinationUser")
                               .Where(txn => txn.TranDestinationAccount == loggedInUser.AccountNumber
                                || txn.TranSourceAccount == loggedInUser.AccountNumber)
                               .ToListAsync();

                var allRange = txnsRange.Where(txn => txn.Date >= startDate && txn.Date <= endDate).ToList();

                foreach (var txn in allRange)
                {
                    if (txn.SourceAccountUserId == null && txn.TranDestinationAccount == loggedInUser.AccountNumber)
                    {
                        txnsByDate.Add(new TransactionListModel
                        {
                            amount = txn.Amount,
                            senderInfo = "P2P Wallet",
                            recepientInfo = $"{txn.DestinationUser.FirstName}-{txn.TranDestinationAccount}",
                            transactionType = "CREDIT",
                            currency = "NGN",
                            status = txn.Status,
                            date = txn.Date,
                        });
                    }

                    if (txn.SourceAccountUserId != null && txn.TranDestinationAccount == loggedInUser.AccountNumber)
                    {
                        txnsByDate.Add(new TransactionListModel
                        {
                            amount = txn.Amount,
                            senderInfo = $"{txn.SourceUser.FirstName}-{txn.TranSourceAccount}",
                            recepientInfo = $"{txn.DestinationUser.FirstName}-{txn.TranDestinationAccount}",
                            transactionType = "CREDIT",
                            currency = "NGN",
                            status = txn.Status,
                            date = txn.Date,
                        });
                    }

                    if (txn.TranSourceAccount == loggedInUser.AccountNumber)
                    {
                        txnsByDate.Add(new TransactionListModel
                        {
                            amount = txn.Amount,
                            senderInfo = $"{txn.SourceUser.FirstName}-{txn.TranSourceAccount}",
                            recepientInfo = $"{txn.DestinationUser.FirstName}-{txn.TranDestinationAccount}",
                            transactionType = "DEBIT",
                            currency = "NGN",
                            status = txn.Status,
                            date = txn.Date,
                        });
                    }

                }

                return txnsByDate;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                _logger.LogInformation("The error occurred at", DateTime.UtcNow.ToLongTimeString());
                return txnsByDate;
            }
        }

        public async Task<CreateStatementViewModel> GeneratePDFStatement(CreateStatementRequestDTO request)
        {
            CreateStatementViewModel createPDFViewModel = new CreateStatementViewModel();
            List<TransactionListModel> txnsByDate = new List<TransactionListModel>();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return createPDFViewModel;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var txnType = "";

                var loggedInUser = await _context.Accounts.Include("User").Where(x => x.UserId == userID).FirstOrDefaultAsync();

                var startDate = Convert.ToDateTime(request.startDate);
                var endDate = Convert.ToDateTime(request.endDate);

                endDate = endDate.AddDays(1);

                var currencyType = await _context.Accounts.Where(x => x.UserId == userID).Select(x => x.Currency).FirstOrDefaultAsync();

                var txnsRange = await _context.Transactions.Include("SourceUser").Include("DestinationUser")
                               .Where(txn => txn.TranDestinationAccount == loggedInUser.AccountNumber
                                || txn.TranSourceAccount == loggedInUser.AccountNumber)
                               .ToListAsync();

                var allRange = txnsRange.Where(txn => txn.Date >= startDate && txn.Date <= endDate 
                && currencyType == request.accountCurrency).ToList();

                CultureInfo culture = new CultureInfo("ig-NG");

                ////////////////////////////////////////////////////////////////////////////////////////////////
                // String builder
                var sb = new StringBuilder();

                sb.Append(@"
                <html>
                    <head>
                        <meta charset='UTF-8'>
                        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                        <title>Globus Wallet Account Statement</title>
                        <link href='https://cdn.jsdelivr.net/npm/bootstrap@5.0.2/dist/css/bootstrap.min.css' rel='stylesheet' integrity='sha384-EVSTQN3/azprG1Anm3QDgpJLIm9Nao0Yz1ztcQTwFspd3yD65VohhpuuCOmLASjC' crossorigin='anonymous'>
                    </head>
                    <body>
                      <div class='container'>
                        <div class='row'>
                          <img src='data:image/png;base64,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' 
                          alt = 'Logo' class='img-fluid imgLogo'>
                        </div>
                        <div class='row'>
                          <div>
                            <div class='dispFlex'>
                              <p class='tableDiv'>Statement Period {StartDate} to {EndDate}</p>
                            </div>
                            <div class='dispFlex'>
                              <table class='tableDiv'>
                                <tr>
                                  <td>Generated Date</td>
                                  <td>{DateTime}</td>
                                </tr>
                                <tr>
                                  <td>Account Number</td>
                                  <td>{AcctNO}</td>
                                </tr>
                                <tr>
                                  <td>Internal Reference</td>
                                  <td>{Reference}</td>
                                </tr>
                                <tr>
                                  <td>Total Debit</td>
                                  <td>{TotalDebit}</td>
                                </tr>
                                <tr>
                                  <td>Total Credit</td>
                                  <td>{TotalCredit}</td>
                                </tr>
                              </table>
                            </div>
                          </div>
                        </div>
                        <div class='row'>
                          <h5>{Name}</h5>
                          <table>
                            <thead>
                              <tr>
                                <th>AMOUNT</th>
                                <th>SENDER INFO</th>
                                <th>RECEPIENT INFO</th>
                                <th>TYPE</th>
                                <th>CURRENCY</th>
                                <th>STATUS</th>
                                <th>DATE</th>
                              </tr>
                            </thead>");
                foreach (var txn in allRange)
                {
                    if (txn.SourceAccountUserId == null && txn.TranDestinationAccount == loggedInUser.AccountNumber)
                    {
                        sb.AppendFormat(@$"
                            <tbody>
                              <tr>
                               <td>{txn.Amount.ToString("C", culture)}</td>
                               <td>P2P Wallet</td>
                               <td>{txn.DestinationUser.FirstName}-{txn.TranDestinationAccount}</td>
                               <td class='crediT'>CREDIT</td>
                               <td>{request.accountCurrency}</td>
                               <td>{txn.Status}</td>
                               <td>{txn.Date.ToString("MM/dd/yyyy hh:mm tt")}</td>
                              </tr>
                            </tbody>");
                    }

                    if (txn.SourceAccountUserId != null && txn.TranDestinationAccount == loggedInUser.AccountNumber)
                    {
                        sb.AppendFormat(@$"
                            <tbody>
                              <tr>
                               <td>{txn.Amount.ToString("C", culture)}</td>
                               <td>{txn.SourceUser.FirstName}-{txn.TranSourceAccount}</td>
                               <td>{txn.DestinationUser.FirstName}-{txn.TranDestinationAccount}</td>
                               <td class='crediT'>CREDIT</td>
                               <td>{request.accountCurrency}</td>
                               <td>{txn.Status}</td>
                               <td>{txn.Date.ToString("MM/dd/yyyy hh:mm tt")}</td>
                              </tr>
                            </tbody>");
                    }

                    if (txn.TranSourceAccount == loggedInUser.AccountNumber)
                    {
                        sb.AppendFormat(@$"
                            <tbody>
                              <tr>
                               <td>{txn.Amount.ToString("C", culture)}</td>
                               <td>{txn.SourceUser.FirstName}-{txn.TranSourceAccount}</td>
                               <td>{txn.DestinationUser.FirstName}-{txn.TranDestinationAccount}</td>
                               <td class='debiT'>DEBIT</td>
                               <td>{request.accountCurrency}</td>
                               <td>{txn.Status}</td>
                               <td>{txn.Date.ToString("MM/dd/yyyy hh:mm tt")}</td>
                              </tr>
                            </tbody>");
                    }
                    ////////////////////////////////////////////////////////////////////////
                    
                }
                
                sb.Append(@"
                            </table>
                        </div>
                      </div>
                    </body>
                </html>");

                var refNO = DateTime.Now.ToString("YYYYMMHHffdd");

                // Info to find and replace
                var dateTime = DateTime.Now.ToString("MM/dd/yyyy hh:mm tt");
                var acctNO = loggedInUser.AccountNumber;
                var reference = $"REF-{refNO}";
                var totalCr = await _context.Transactions.Where(x => x.TranDestinationAccount == loggedInUser.AccountNumber).Select(x => x.Amount).SumAsync();
                var totalDr = await _context.Transactions.Where(x => x.TranSourceAccount == loggedInUser.AccountNumber).Select(x => x.Amount).SumAsync();
                var name = $"{loggedInUser.User.FirstName} {loggedInUser.User.LastName}";
                /////////////////////////////////////////////////////////////////////////////////////////////

                sb.Replace("{StartDate}", request.startDate);
                sb.Replace("{EndDate}", request.endDate);
                sb.Replace("{DateTime}", dateTime);
                sb.Replace("{AcctNO}", acctNO);
                sb.Replace("{Reference}", reference);
                sb.Replace("{TotalDebit}", totalCr.ToString("C", culture));
                sb.Replace("{TotalCredit}", totalDr.ToString("C", culture));
                sb.Replace("{Name}", name);

                ////////////////////////////////////////////////////////////////////////////////////////////////
                var globalSettings = new GlobalSettings
                {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                    Margins = new MarginSettings { Top = 10 },
                    DocumentTitle = "Globus Wallet Statement",
                    Out = @$"C:\Users\joyihama\Desktop\StatementReport\{loggedInUser.User.Username}_GlobusWallet_Statement.pdf"
                };

                string path1 = "C:\\Users\\joyihama\\Documents\\Projects\\WalletPaymentApp\\WalletPayment.Services";
                string path2 = "Assets\\Styles.css";


                var objectSettings = new ObjectSettings
                {
                    PagesCount = true,
                    HtmlContent = sb.ToString(),
                    WebSettings = {
                        DefaultEncoding = "utf-8",
                        UserStyleSheet = Path.Combine(path1, path2)
                    },
                    HeaderSettings = { FontName = "Poppins", FontSize = 9, Right = "Page [page] of [toPage]", Line = true },
                    FooterSettings = { FontName = "Poppins", FontSize = 9, Line = true, Center = "Globus Wallet Statement" }
                };

                var pdf = new HtmlToPdfDocument()
                {
                    GlobalSettings = globalSettings,
                    Objects = { objectSettings }
                };

                byte[] convertedPDFToBytes = _converter.Convert(pdf);


                var stream = new MemoryStream(convertedPDFToBytes);
                var copiedStream = new MemoryStream();

                stream.CopyTo(copiedStream);
                copiedStream.Position = 0;


                ///////////////////////////////////////////////////////////////////////////////////////////////
                // Send pdf to user email
                var userName = loggedInUser.User.Username;
                var userEmail = loggedInUser.User.Email;

                IFormFile formFileAttachment = new FormFile(copiedStream, 0, copiedStream.Length, null, $"{loggedInUser.User.Username}_GlobusWallet_Statement.pdf")
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "application/pdf"
                };


                //var result = await _emailService.SendStatementAsAttachment(userName, userEmail, formFileAttachment);

                //if (result == false)
                //{
                //    createPDFViewModel.status = false;
                //    createPDFViewModel.message = "An error occured. Could not send to email!";
                //    return createPDFViewModel;
                //}

                /////////////////////////////////////////////////////////////////////////////////////////////////

                createPDFViewModel.status = true;
                createPDFViewModel.message = "Successfully created PDF document!! \r\n Please check your email to view document.";
                return createPDFViewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                _logger.LogInformation("The error occurred at",
                    DateTime.UtcNow.ToLongTimeString());
                return createPDFViewModel;
            }
        }


        private void CreateCell(IRow CurrentRow, int CellIndex, string Value, HSSFCellStyle Style)
        {
            ICell Cell = CurrentRow.CreateCell(CellIndex);
            Cell.SetCellValue(Value);
            Cell.CellStyle = Style;
        }

        public async Task<CreateStatementViewModel> GenerateExcelStatement(CreateStatementRequestDTO request)
        {
            CreateStatementViewModel createExcelViewModel = new CreateStatementViewModel();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return createExcelViewModel;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var txnType = "";

                var loggedInUser = await _context.Accounts.Include("User").Where(x => x.UserId == userID).FirstOrDefaultAsync();

                var startDate = Convert.ToDateTime(request.startDate);
                var endDate = Convert.ToDateTime(request.endDate);

                endDate = endDate.AddDays(1);

                var currencyType = await _context.Accounts.Where(x => x.UserId == userID).Select(x => x.Currency).FirstOrDefaultAsync();

                var txnsRange = await _context.Transactions.Include("SourceUser").Include("DestinationUser")
                               .Where(txn => txn.TranDestinationAccount == loggedInUser.AccountNumber
                                || txn.TranSourceAccount == loggedInUser.AccountNumber)
                               .ToListAsync();

                var allRange = txnsRange.Where(txn => txn.Date >= startDate && txn.Date <= endDate
                && currencyType == request.accountCurrency).ToList();

                CultureInfo culture = new CultureInfo("ig-NG");

                //////////////////////////////////////////////////////////////////////////////////////////////////

                HSSFWorkbook workbook = new HSSFWorkbook();
                HSSFFont myFont = (HSSFFont)workbook.CreateFont();
                myFont.FontHeightInPoints = 12;
                myFont.FontName = "Poppins";
                myFont.Color = HSSFColor.DarkBlue.Index;

                ISheet sheet = workbook.CreateSheet($"{loggedInUser.User.Username}_GlobusWallet_Sheet1");

                // Defining a border
                HSSFCellStyle borderedCellStyle = (HSSFCellStyle)workbook.CreateCellStyle();
                borderedCellStyle.SetFont(myFont);
                borderedCellStyle.BorderLeft = BorderStyle.Medium;
                borderedCellStyle.BorderTop = BorderStyle.Medium;
                borderedCellStyle.BorderRight = BorderStyle.Medium;
                borderedCellStyle.BorderBottom = BorderStyle.Medium;
                borderedCellStyle.VerticalAlignment = VerticalAlignment.Center;
                borderedCellStyle.LeftBorderColor = HSSFColor.DarkRed.Index;
                borderedCellStyle.TopBorderColor = HSSFColor.DarkRed.Index;
                borderedCellStyle.RightBorderColor = HSSFColor.DarkRed.Index;
                borderedCellStyle.BottomBorderColor = HSSFColor.DarkRed.Index;
                borderedCellStyle.FillBackgroundColor = HSSFColor.White.Index;
                borderedCellStyle.FillForegroundColor = HSSFColor.White.Index;
                borderedCellStyle.FillPattern = FillPattern.SolidForeground;

                IRow headerRow = sheet.CreateRow(7);
                CreateCell(headerRow, 0, "AMOUNT", borderedCellStyle);
                CreateCell(headerRow, 1, "SENDER INFO", borderedCellStyle);
                CreateCell(headerRow, 2, "RECEPIENT INFO", borderedCellStyle);
                CreateCell(headerRow, 3, "TYPE", borderedCellStyle);
                CreateCell(headerRow, 4, "CURRENCY", borderedCellStyle);
                CreateCell(headerRow, 5, "STATUS", borderedCellStyle);
                CreateCell(headerRow, 6, "DATE", borderedCellStyle);


                int rowIncr = 8;

                foreach (var txn in allRange)
                {
                    if (txn.SourceAccountUserId == null && txn.TranDestinationAccount == loggedInUser.AccountNumber)
                    {
                        IRow dataRow = sheet.CreateRow(rowIncr++);
                        CreateCell(dataRow, 0, $"{txn.Amount.ToString("C", culture)}", borderedCellStyle);
                        CreateCell(dataRow, 1, "P2P Wallet", borderedCellStyle);
                        CreateCell(dataRow, 2, $"{txn.DestinationUser.FirstName}-{txn.TranDestinationAccount}", borderedCellStyle);
                        CreateCell(dataRow, 3, "CREDIT", borderedCellStyle);
                        CreateCell(dataRow, 4, $"{request.accountCurrency}", borderedCellStyle);
                        CreateCell(dataRow, 5, $"{txn.Status}", borderedCellStyle);
                        CreateCell(dataRow, 6, $"{txn.Date.ToString("MM/dd/yyyy hh:mm tt")}", borderedCellStyle);
                    }

                    if (txn.SourceAccountUserId != null && txn.TranDestinationAccount == loggedInUser.AccountNumber)
                    {
                        IRow dataRow = sheet.CreateRow(rowIncr++);
                        CreateCell(dataRow, 0, $"{txn.Amount.ToString("C", culture)}", borderedCellStyle);
                        CreateCell(dataRow, 1, $"{txn.SourceUser.FirstName}-{txn.TranSourceAccount}", borderedCellStyle);
                        CreateCell(dataRow, 2, $"{txn.DestinationUser.FirstName}-{txn.TranDestinationAccount}", borderedCellStyle);
                        CreateCell(dataRow, 3, "CREDIT", borderedCellStyle);
                        CreateCell(dataRow, 4, $"{request.accountCurrency}", borderedCellStyle);
                        CreateCell(dataRow, 5, $"{txn.Status}", borderedCellStyle);
                        CreateCell(dataRow, 6, $"{txn.Date.ToString("MM/dd/yyyy hh:mm tt")}", borderedCellStyle);
                    }

                    if (txn.TranSourceAccount == loggedInUser.AccountNumber)
                    {
                        IRow dataRow = sheet.CreateRow(rowIncr++);
                        CreateCell(dataRow, 0, $"{txn.Amount.ToString("C", culture)}", borderedCellStyle);
                        CreateCell(dataRow, 1, $"{txn.SourceUser.FirstName}-{txn.TranSourceAccount}", borderedCellStyle);
                        CreateCell(dataRow, 2, $"{txn.DestinationUser.FirstName}-{txn.TranDestinationAccount}", borderedCellStyle);
                        CreateCell(dataRow, 3, "DEBIT", borderedCellStyle);
                        CreateCell(dataRow, 4, $"{request.accountCurrency}", borderedCellStyle);
                        CreateCell(dataRow, 5, $"{txn.Status}", borderedCellStyle);
                        CreateCell(dataRow, 6, $"{txn.Date.ToString("MM/dd/yyyy hh:mm tt")}", borderedCellStyle);
                    }
                }


                var getImagePath = Path.GetFullPath("C:\\Users\\joyihama\\Desktop\\logo.png");
                byte[] imageBytes;
                //byte[] imageBytes = File.ReadAllBytes(getImagePath);

                using (FileStream fileStream = new FileStream(getImagePath, FileMode.Open, FileAccess.Read))
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        fileStream.CopyTo(memoryStream);
                        imageBytes = memoryStream.ToArray();
                    }
                }

                int pictureIndex = workbook.AddPicture(imageBytes, PictureType.PNG);
                HSSFPatriarch patriarch = (HSSFPatriarch)sheet.CreateDrawingPatriarch();
                IClientAnchor anchor = workbook.GetCreationHelper().CreateClientAnchor();

                anchor.Row1 = 0;
                anchor.Col1 = 0;
                anchor.Dx1 = 0;
                anchor.Dy1 = 0;

                anchor.Dx2 = 500;
                anchor.Dy2 = 500;

                IPicture picture = patriarch.CreatePicture(anchor, pictureIndex);
                //picture.Resize();

                CellRangeAddress mergedRegion = new CellRangeAddress(0, 6, 0, 6);
                sheet.AddMergedRegion(mergedRegion);


                // Save the workbook to a file
                using (FileStream fileStream = new FileStream($"C:/Users/joyihama/Desktop/StatementReport/{loggedInUser.User.Username}_GlobusWallet_Sheet.xls", FileMode.Create))
                {
                    workbook.Write(fileStream, false);
                }

                workbook.Close();
                workbook.Dispose();


                createExcelViewModel.status = true;
                createExcelViewModel.message = "Successfully created Excel document";
                return createExcelViewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                _logger.LogInformation("The error occurred at",
                    DateTime.UtcNow.ToLongTimeString());
                return createExcelViewModel;
            }
        }




    }
}














