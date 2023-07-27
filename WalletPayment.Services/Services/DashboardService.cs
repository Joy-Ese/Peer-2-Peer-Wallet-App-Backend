using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using WalletPayment.Models.DataObjects;
using WalletPayment.Models.Entites;
using WalletPayment.Services.Data;
using WalletPayment.Services.Interfaces;

namespace WalletPayment.Services.Services
{
    public class DashboardService : IDashboard
    {
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<DashboardService> _logger;
        private readonly IWebHostEnvironment _environment;

        public DashboardService(DataContext context, IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor, ILogger<DashboardService> logger)
        {
            _context = context;
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _logger.LogDebug(1, "Nlog injected into DashboardService");
        }

        public async Task<UserDashboardViewModel> GetUserDetails()
        {
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return new UserDashboardViewModel();
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var getAcctList = new List<AccountDetails>();
               
                var acctData = await _context.Accounts.Where(x => x.UserId == userID).ToListAsync();

                foreach (var item in acctData)
                {
                    var accountDetails = new AccountDetails()
                    {
                        AccountNumber = item.AccountNumber,
                        Balance = item.Balance,
                        Currency = item.Currency,
                    };
                    getAcctList.Add(accountDetails);
                }

                var userData = await _context.Users.Include("UserAccount")
                                .Where(userInfo => userInfo.Id == userID)
                                .Select(userInfo => new UserDashboardViewModel
                                {
                                    Username = userInfo.Username,
                                    FirstName = userInfo.FirstName,
                                    LastName = userInfo.LastName,
                                    AccountDetails = getAcctList,
                                    Email = userInfo.Email,
                                    PhoneNumber = userInfo.PhoneNumber,
                                    Address = userInfo.Address,
                                })
                                .FirstOrDefaultAsync();

                if (userData == null) return new UserDashboardViewModel();

                return userData;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return new UserDashboardViewModel();
            }
        }

        public async Task<UserBalanceViewModel> GetUserAccountBalance()
        {
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return new UserBalanceViewModel();
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var getAcctList = new List<AccountDetails>();
                var accountDetails = new AccountDetails();
                var acctData = await _context.Accounts.Where(x => x.UserId == userID).ToListAsync();


                foreach (var item in acctData)
                {
                    accountDetails.AccountNumber = item.AccountNumber;
                    accountDetails.Balance = item.Balance;
                    accountDetails.Currency = item.Currency;
                    getAcctList.Add(accountDetails);
                }

                var acctBalance = await _context.Users
                                .Where(userAccBal => userAccBal.Id == userID)
                                .Select(userAccBal => new UserBalanceViewModel
                                {
                                    AccountDetails = getAcctList
                                })
                                .FirstOrDefaultAsync();

                if (acctBalance == null) return new UserBalanceViewModel();

                return acctBalance;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return new UserBalanceViewModel();
            }
        }

        public async Task<UserAcctNumViewModel> GetUserAccountNumber()
        {
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return new UserAcctNumViewModel();
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var getAcctList = new List<AccountDetails>();
                var accountDetails = new AccountDetails();
                var acctData = await _context.Accounts.Where(x => x.UserId == userID).ToListAsync();


                foreach (var item in acctData)
                {
                    accountDetails.AccountNumber = item.AccountNumber;
                    accountDetails.Balance = item.Balance;
                    accountDetails.Currency = item.Currency;
                    getAcctList.Add(accountDetails);
                }

                var acctNumber = await _context.Users
                                .Where(userAccNum => userAccNum.Id == userID)
                                .Select(userAccNum => new UserAcctNumViewModel
                                {
                                    AccountDetails = getAcctList
                                })
                                .FirstOrDefaultAsync();

                if (acctNumber == null) return new UserAcctNumViewModel();

                return acctNumber;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return new UserAcctNumViewModel();
            }
        }

        public async Task<UserAcctCurrencyModel> GetUserAccountCurrency()
        {
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return new UserAcctCurrencyModel();
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var getAcctList = new List<AccountDetails>();
                var accountDetails = new AccountDetails();
                var acctData = await _context.Accounts.Where(x => x.UserId == userID).ToListAsync();


                foreach (var item in acctData)
                {
                    accountDetails.AccountNumber = item.AccountNumber;
                    accountDetails.Balance = item.Balance;
                    accountDetails.Currency = item.Currency;
                    getAcctList.Add(accountDetails);
                }

                var acctCurr = await _context.Users
                                .Where(userAccNum => userAccNum.Id == userID)
                                .Select(userAccNum => new UserAcctCurrencyModel
                                {
                                    AccountDetails = getAcctList
                                })
                                .FirstOrDefaultAsync();

                if (acctCurr == null) return new UserAcctCurrencyModel();

                return acctCurr;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return new UserAcctCurrencyModel();
            }
        }

        public async Task<UserDashboardViewModel> GetUserEmail()
        {
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return new UserDashboardViewModel();
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var userEmail = await _context.Users
                                .Where(uEmail => uEmail.Id == userID)
                                .Select(uEmail => new UserDashboardViewModel
                                {
                                    Email = uEmail.Email,
                                })
                                .FirstOrDefaultAsync();

                if (userEmail == null) return new UserDashboardViewModel();

                return userEmail;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return new UserDashboardViewModel();
            }
        }

        public async Task<UserProfileViewModel> GetUserProfile()
        {
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return new UserProfileViewModel();
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var userPROFILE = await _context.Users
                                .Where(p => p.Id == userID)
                                .Select(p => new UserProfileViewModel
                                {
                                    firstName = p.FirstName,
                                    lastName = p.LastName,
                                    username = p.Username,
                                    phoneNumber = p.PhoneNumber,
                                    email = p.Email,
                                    address = p.Address,
                                })
                                .FirstOrDefaultAsync();

                if (userPROFILE == null) return new UserProfileViewModel();

                return userPROFILE;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return new UserProfileViewModel();
            }
        }

        public async Task<UpdateUserInfoModel> UpdateUserInfo(UpdateUserInfoDto request)
        {
            UpdateUserInfoModel updateUserInfo = new UpdateUserInfoModel();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return updateUserInfo;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var userUpdated = await _context.Users.FindAsync(userID);
                if (userUpdated == null)
                {
                    updateUserInfo.message = "User's Information could not be updated!";
                    return updateUserInfo;
                }

                userUpdated.FirstName = request.firstName;
                userUpdated.LastName = request.lastName;
                userUpdated.Username = request.username;
                userUpdated.Email = request.email;
                userUpdated.PhoneNumber = request.phoneNumber;
                userUpdated.Address = request.address;

                await _context.SaveChangesAsync();

                updateUserInfo.status = true;
                updateUserInfo.message = "User Information successfully updated!";
                return updateUserInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return updateUserInfo;
            }
        }

        public async Task<ImageRequestViewModel> UploadNewImage(IFormFile fileData)
        {
            ImageRequestViewModel imageRequestViewModel = new ImageRequestViewModel();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return imageRequestViewModel;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                Image newImageUpload = new Image
                {
                    UserId = userID,
                    FileName = fileData.FileName,
                    TimeUploaded = DateTime.Now,
                };

                using (var stream = new MemoryStream())
                {
                    fileData.CopyTo(stream);
                    newImageUpload.FileData = stream.ToArray();
                }

                await _context.Images.AddAsync(newImageUpload);
                var result = await _context.SaveChangesAsync();

                if (result < 0)
                {
                    imageRequestViewModel.status = false;
                    imageRequestViewModel.message = "Image could not be uploaded";
                    return imageRequestViewModel;
                }

                imageRequestViewModel.status = true;
                imageRequestViewModel.message = "Image uploaded successfully";
                return imageRequestViewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return imageRequestViewModel;
            }
        }

        public async Task<ImageRequestViewModel> UpdateImage(IFormFile fileData)
        {
            ImageRequestViewModel imageRequestViewModel = new ImageRequestViewModel();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return imageRequestViewModel;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var userImageToUpdate = await _context.Images.Where(x => x.UserId == userID).FirstOrDefaultAsync();
                if (userImageToUpdate == null)
                {
                    imageRequestViewModel.message = "User's Image could not be updated!";
                    return imageRequestViewModel;
                }

                userImageToUpdate.FileName = fileData.FileName;
                userImageToUpdate.TimeUploaded = DateTime.Now;
                
                using (var stream = new MemoryStream())
                {
                    fileData.CopyTo(stream);
                    userImageToUpdate.FileData = stream.ToArray();
                }

                var result = await _context.SaveChangesAsync();

                if (result < 0)
                {
                    imageRequestViewModel.status = false;
                    imageRequestViewModel.message = "Image could not be updated!";
                    return imageRequestViewModel;
                }

                imageRequestViewModel.status = true;
                imageRequestViewModel.message = "Image updated successfully!";
                return imageRequestViewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return imageRequestViewModel;
            }
        }

        public async Task<ImageViewModel> GetUserImage()
        {
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return new ImageViewModel();
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var image = await _context.Images
                                .Where(userImg => userImg.UserId == userID)
                                .Select(userImg => new ImageViewModel
                                {
                                    imageDetails = userImg.FileData
                                })
                                .FirstOrDefaultAsync();

                if (image == null) return new ImageViewModel();

                return image;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return new ImageViewModel();
            }
        }

        public async Task<DeleteImageViewModel> DeleteUserImage()
        {
            DeleteImageViewModel deleteImage = new DeleteImageViewModel();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return deleteImage;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var image = await _context.Images.Where(userImg => userImg.UserId == userID).FirstOrDefaultAsync();

                _context.Images.Remove(image);
                await _context.SaveChangesAsync();

                if (image == null)
                {
                    deleteImage.message = "Image could not be deleted";
                    return deleteImage;
                }

                deleteImage.status = true;
                deleteImage.message = "Image successfully deleted";
                return deleteImage;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return deleteImage;
            }
        }

        public async Task<SecurityQuestionViewModel> SetSecurityQuestion(SecurityQuestionDto request)
        {
            SecurityQuestionViewModel securityResponse = new SecurityQuestionViewModel();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return securityResponse;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                if (string.IsNullOrEmpty(request.question) || string.IsNullOrEmpty(request.answer))
                {
                    securityResponse.message = "Question or Answer cannot be empty";
                    return securityResponse;
                }

                SecurityQuestion securityQuestion = new SecurityQuestion
                {
                    Question = request.question,
                    Answer = request.answer.ToLower(),
                    UserId = userID

                };

                await _context.SecurityQuestions.AddAsync(securityQuestion);
                await _context.SaveChangesAsync();

                securityResponse.status = true;
                securityResponse.message = "Your answer has been saved";
                return securityResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return securityResponse;
            }
        }

        public async Task<GetSecurityQuestionViewModel> GetUserSecurityQuestion()
        {
            GetSecurityQuestionViewModel model = new GetSecurityQuestionViewModel();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return model;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var userSecQuest = await _context.SecurityQuestions.Where(s => s.UserId == userID)
                                        .Select(s => new GetSecurityQuestionViewModel
                                        {
                                            Question = s.Question
                                        }).FirstOrDefaultAsync();

                if (userSecQuest == null) return model;

                return userSecQuest;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return model;
            }
        }

        public async Task<bool> GetUserSecurityAnswer()
        {
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return false;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var userSecAns = await _context.SecurityQuestions.Where(s => s.UserId == userID).FirstOrDefaultAsync();

                //if (userSecAns.Answer == null) return false;
                if (userSecAns == null) return false;

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetUserPin()
        {
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return false;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var userPin = await _context.Users.Where(s => s.Id == userID).FirstOrDefaultAsync();

                if (userPin.PinSalt == null) return false;
                if (userPin.PinHash == null) return false;

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DoesUserHaveImage()
        {
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return false;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var userImg = await _context.Images.Where(s => s.UserId == userID).FirstOrDefaultAsync();

                if (userImg == null) return false;

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return false;
            }
        }

        public async Task<bool> NoSecurityAttemptsLeft()
        {
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return false;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var userSec = await _context.SecurityQuestions.Where(x => x.UserId == userID).FirstOrDefaultAsync();

                if (userSec.Attempts == 0)
                {
                    return true;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return false;
            }
        }

        public async Task<KycViewModel> KycUpload(IFormFile fileData)
        {
            KycViewModel kycRequestViewModel = new KycViewModel();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return kycRequestViewModel;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);
                
                var loggedInUser = await _context.Users.Where(x => x.Id == userID).FirstOrDefaultAsync();

                var userUniqueFileRef = loggedInUser.Username;

                string extractPath = @"C:\Users\joyihama\Desktop\KYCGlobusWallet";
                
                string zipFileName = $"{userUniqueFileRef}.zip";

                using (var target = File.Create($"{userUniqueFileRef}.zip"))
                {
                    using (var zipArchive = new ZipArchive(target, ZipArchiveMode.Create, true))
                    {
                        var zipEntry = zipArchive.CreateEntry(fileData.FileName, CompressionLevel.Optimal);

                        using (var zipStream = zipEntry.Open())
                        {
                            await fileData.CopyToAsync(zipStream);
                        }
                    }
                }
                using (var zipExtract = new ZipArchive(File.OpenRead(zipFileName), ZipArchiveMode.Read))
                {
                    foreach (var entry in zipExtract.Entries)
                    {
                        if (!string.IsNullOrEmpty(entry.Name))
                        {
                            string extractFilePath = Path.Combine(extractPath, entry.FullName);
                            entry.ExtractToFile(extractFilePath, true);
                        }
                    }
                }

                File.Delete($"{userUniqueFileRef}.zip");

                string fileName = fileData.FileName;
                string filePath = GetFilePath();

                if (!System.IO.Directory.Exists(filePath))
                {
                    System.IO.Directory.CreateDirectory(filePath);
                }

                string imagePath = filePath + fileName;

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
                using (FileStream stream = System.IO.File.Create(imagePath))
                {
                    await fileData.CopyToAsync(stream);
                }

                KycImage kyc = new KycImage
                {
                    TimeUploaded = DateTime.Now,
                    UserId = userID,
                    FileName = $"{userID}{fileData.FileName}",
                    IsAccepted = false
                };

                await _context.KycImages.AddAsync(kyc);
                var result = await _context.SaveChangesAsync();

                if (result < 0)
                {
                    kycRequestViewModel.status = false;
                    kycRequestViewModel.message = "Image could not be uploaded";
                    return kycRequestViewModel;
                }

                kycRequestViewModel.status = true;
                kycRequestViewModel.message = "Thank you for submitting your document. Account will be upgraded!";
                return kycRequestViewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return kycRequestViewModel;
            }
        }

        public async Task<KycViewModel> KycReUpload(IFormFile fileData)
        {
            KycViewModel kycRequestViewModel = new KycViewModel();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return kycRequestViewModel;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var kycImageToUpdate = await _context.KycImages.Where(x => x.UserId == userID).FirstOrDefaultAsync();

                string fileName = fileData.FileName;
                string filePath = GetFilePath();

                if (!System.IO.Directory.Exists(filePath))
                {
                    System.IO.Directory.CreateDirectory(filePath);
                }

                string imagePath = filePath + fileName;

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
                using (FileStream stream = System.IO.File.Create(imagePath))
                {
                    await fileData.CopyToAsync(stream);
                }

                KycImage kyc = new KycImage
                {
                    TimeUploaded = DateTime.Now,
                    UserId = userID,
                    FileName = $"{userID}{fileData.FileName}",
                    IsAccepted = false
                };

                kycImageToUpdate.FileName = fileData.FileName;
                kycImageToUpdate.TimeUploaded = DateTime.Now;

                var result = await _context.SaveChangesAsync();

                if (result < 0)
                {
                    kycRequestViewModel.status = false;
                    kycRequestViewModel.message = "Image could not be re-uploaded";
                    return kycRequestViewModel;
                }

                kycRequestViewModel.status = true;
                kycRequestViewModel.message = "Thank you for submitting your document. Account will be upgraded!";
                return kycRequestViewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return kycRequestViewModel;
            }
        }

        private string GetFilePath()
        {
            return _environment.WebRootPath + $"\\KycUploads\\GovtIssued\\";
        }

        private string GetImage()
        {
            string imageUrl = string.Empty;
            string hostUrl = "http://localhost:7236";
            string filePath = GetFilePath();

            if (!System.IO.Directory.Exists(filePath))
            {
                imageUrl = hostUrl + "/kycUploads/common/download.png";
            }
            else
            {
                imageUrl = hostUrl + $"/kycUploads/GovtIssued/";
            }

            return imageUrl;
        }

        public async Task<List<UserInfoOnKycUploadsForAdminModel>> GetUserInfoOnKycUploadsForAdmin()
        {
            List<UserInfoOnKycUploadsForAdminModel> kycs = new List<UserInfoOnKycUploadsForAdminModel>();
            try
            {
                var getPendingKyc = await _context.KycImages.Where(x => x.IsAccepted == false).ToListAsync();

                foreach (var kyc in getPendingKyc)
                {
                    var userDetail = await _context.Users.Where(x => x.Id == kyc.UserId).FirstOrDefaultAsync();
                    kycs.Add(new UserInfoOnKycUploadsForAdminModel
                    {
                        firstname = userDetail.FirstName,
                        lastname = userDetail.LastName,
                    });
                }

                return kycs;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return new List<UserInfoOnKycUploadsForAdminModel>();
            }
        }

        public async Task<List<KycAdminViewModel>> GetKycUploadsForAdmin()
        {
            List<KycAdminViewModel> kycs = new List<KycAdminViewModel>();
            try
            {
                string getRole;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return new List<KycAdminViewModel>();
                }

                var imagesUploaded = await _context.KycImages.ToListAsync();

                if (imagesUploaded == null && imagesUploaded.Count == 0)
                {
                    return new List<KycAdminViewModel>();
                }

                foreach (var image in imagesUploaded)
                {
                    var user = await _context.Users.Where(x => x.Id == image.UserId).FirstOrDefaultAsync();

                    string imgUrl = GetImage();
                    string url = $"{imgUrl}{image.FileName}";

                    kycs.Add(new KycAdminViewModel
                    {
                        image = url,
                        filename = image.FileName,
                        timeUploaded = image.TimeUploaded,
                        kycUploader = user.Username,
                        userId = image.UserId,
                        isAccepted = image.IsAccepted,
                    });
                }

                return kycs;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return new List<KycAdminViewModel>();
            }
        }

        public async Task<KycViewModel> RemoveImage(string filename, string userId)
        {
            KycViewModel kycRemove = new KycViewModel();
            try
            {
                int userID;
                userID = Convert.ToInt32(userId);

                var user = await _context.Users.Where(x => x.Id == userID).FirstOrDefaultAsync();
                var fileNAME = await _context.KycImages.Where(x => x.FileName == filename).FirstOrDefaultAsync();

                string filePath = GetFilePath();
                string imagePath = filePath + fileNAME.FileName;

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                Notification newNotification = new Notification
                {
                    Title = $"Kyc Validation Failed",
                    Description = $"You need to redo your kyc validation. One or more documents found wanting!!",
                    Date = DateTime.Now,
                    NotificationUserId = userID,
                    IsNotificationRead = false,
                };

                await _context.Notifications.AddAsync(newNotification);
                await _context.SaveChangesAsync();

                fileNAME.IsAccepted = false;
                fileNAME.IsRejected = true;
                fileNAME.TimeUploaded = DateTime.Now;
                await _context.SaveChangesAsync();

                kycRemove.status = true;
                kycRemove.message = $"Rejected {user.Username}'s documents";
                return kycRemove;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return kycRemove;
            }
        }

        public async Task<KycViewModel> AcceptImage(string filename, string userId)
        {
            KycViewModel kycAccept = new KycViewModel();
            try
            {
                int userID;
                userID = Convert.ToInt32(userId);

                var user = await _context.Users.Where(x => x.Id == userID).FirstOrDefaultAsync();
                var fileNAME = await _context.KycImages.Where(x => x.FileName == filename).FirstOrDefaultAsync();

                string filePath = GetFilePath();
                string imagePath = filePath + fileNAME.FileName;

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                user.UserProfile = "Verified";
                await _context.SaveChangesAsync();

                Notification newNotification = new Notification
                {
                    Title = $"Kyc Validation Successful",
                    Description = $"Your documents have been accepted. You can now create foreign wallets.",
                    Date = DateTime.Now,
                    NotificationUserId = userID,
                    IsNotificationRead = false,
                };


                await _context.Notifications.AddAsync(newNotification);
                await _context.SaveChangesAsync();

                fileNAME.IsAccepted = true;
                fileNAME.IsRejected = false;
                fileNAME.TimeUploaded = DateTime.Now;
                await _context.SaveChangesAsync();

                kycAccept.status = true;
                kycAccept.message = $"Accepted {user.Username}'s documents";
                return kycAccept;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return kycAccept;
            }
        }

        public async Task<UserProfileModel> GetUserProfileLevel()
        {
            UserProfileModel userProfile = new UserProfileModel();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return userProfile;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var info = await _context.Users.Where(x => x.Id == userID).FirstOrDefaultAsync();

                userProfile.status = true;
                userProfile.message = info.UserProfile;
                return userProfile;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return userProfile;
            }
        }
        
        public async Task<List<AllAdminsListModel>> AllAdminsLists()
        {
            List<AllAdminsListModel> allAdmins = new List<AllAdminsListModel>();
            try
            {
                string getRole;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return allAdmins;
                }

                getRole = _httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.Role)?.Value;

                if (getRole != "SuperAdmin")
                {
                    return allAdmins;
                }

                var adminss = await _context.Adminss.ToListAsync();

                foreach (var admin in adminss)
                {
                    allAdmins.Add(new AllAdminsListModel
                    {
                        username = admin.Username,
                        role = admin.Role,
                        email = admin.Email,
                        isDisabled = admin.IsDisabled,
                        id = admin.Id,
                    });
                }
                return allAdmins;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return new List<AllAdminsListModel>();
            }
        }

        public async Task<ResponseModel> DisableEnableAdmin(DisableEnableAdminDTO req)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                string getRole;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return response;
                }

                getRole = _httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.Role)?.Value;

                if (getRole != "SuperAdmin")
                {
                    response.status = false;
                    response.message = "You are not authourized!!!!";
                    return response;
                }

                var adminClicked = await _context.Adminss.Where(x => x.Id == req.id).FirstOrDefaultAsync();

                if (adminClicked.IsDisabled == false)
                {
                    adminClicked.IsDisabled = true;
                    await _context.SaveChangesAsync();

                    response.status = true;
                    response.message = "Successfully Disabled";
                    return response;
                }

                adminClicked.IsDisabled = false;
                await _context.SaveChangesAsync();

                response.status = true;
                response.message = "Successfully Enabled";
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return response;
            }
        }

        public async Task<bool> GetKycStatus()
        {
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return false;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var userKycStatus = await _context.KycImages.Where(s => s.UserId == userID).FirstOrDefaultAsync();

                if (userKycStatus == null) return false;

                return userKycStatus.IsRejected;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return false;
            }
        }


    }
}





