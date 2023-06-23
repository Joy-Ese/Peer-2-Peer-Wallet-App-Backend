﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Expressions;
using System.Linq.Dynamic;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WalletPayment.Models.DataObjects;
using WalletPayment.Models.Entites;
using WalletPayment.Services.Data;
using WalletPayment.Services.Interfaces;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.WebUtilities;
using WalletPayment.Models.DataObjects.Common;

namespace WalletPayment.Services.Services
{
    public class AuthService : IAuth
    {
        private readonly DataContext _context;
        private readonly LinkGenerator _linkGenerator;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;
        private readonly IEmail _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuthService> _logger;
        private readonly FrontEndResetDetail? _resetLink;

        public AuthService(DataContext context, IEmail emailService, IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor, LinkGenerator linkGenerator, ILogger<AuthService> logger, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _linkGenerator = linkGenerator;
            _webHostEnvironment = webHostEnvironment;
            _resetLink = configuration.GetSection("FrontEndResetDetails").Get<FrontEndResetDetail>();
            _logger = logger;
            _logger.LogDebug(1, "Nlog injected into AuthService");
        }

        public string AccountNumberGenerator()
        {
            var accountNumber = DateTime.Now.ToString("HHffMMffdd");
            return accountNumber;
        }

        public async Task<AdminViewModel> CreateAdmin(CreateAdminDTO request)
        {
            AdminViewModel createAdminResponse = new AdminViewModel();
            try
            {
                string getRole;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return createAdminResponse;
                }

                getRole = _httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.Role)?.Value;

                if (getRole != "Admin")
                {
                    createAdminResponse.message = "Sorry!!Only an authourized Admin can create another Admin";
                    return createAdminResponse;
                }


                CreatePasswordHash(request.password, out byte[] passwordHash, out byte[] passwordSalt);

                var adminUsername = "GlobusWalletAdmin";

                Admin newAdmin = new Admin
                {
                    Username = adminUsername,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                };

                await _context.Admins.AddAsync(newAdmin);
                await _context.SaveChangesAsync();

                //SystemAccount sysAccount = new SystemAccount
                //{
                //    SystemBalance = 0,
                //    Currency = "NGN",
                //};

                //await _context.SystemAccounts.AddAsync(sysAccount);
                //await _context.SaveChangesAsync();

                createAdminResponse.status = true;
                createAdminResponse.message = $"Admin successfully created!!! Admin can login with username:{adminUsername} and set their password";
                return createAdminResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                _logger.LogInformation("The error occurred at",
                    DateTime.UtcNow.ToLongTimeString());
                return createAdminResponse;
            }
        }

        public async Task<RegisterViewModel> Register(UserSignUpDto request)
        {
            RegisterViewModel registerResponse = new RegisterViewModel();
            try
            {
                var data = await _context.Users
                .AnyAsync(user => user.Username == request.username || user.Email == request.email);

                if (data)
                {
                    _logger.LogWarning($"Duplicate username/email supplied {request.username}/{request.email}");
                    registerResponse.message = "Duplicate username or email";
                    return registerResponse;
                }

                if (!request.password.Equals(request.confirmPassword))
                {
                    registerResponse.message = "Passwords do not match";
                    return registerResponse;
                }

                CreatePasswordHash(request.password, out byte[] passwordHash, out byte[] passwordSalt);

                string generatedAcc = AccountNumberGenerator();
                string randomToken = CreateRandomToken();

                User newUser = new User
                {
                    Username = request.username,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    Email = request.email,
                    PhoneNumber = request.phoneNumber,
                    FirstName = request.firstName,
                    LastName = request.lastName,
                    Address = request.address,
                    IsUserLocked = false,
                    VerificationToken = randomToken,
                };
                await _context.Users.AddAsync(newUser);
                await _context.SaveChangesAsync();


                Account newAccount = new Account
                {
                    AccountNumber = generatedAcc,
                    Balance = 0,
                    Currency = "NGN",
                    UserId = newUser.Id
                };

                await _context.Accounts.AddAsync(newAccount);
                await _context.SaveChangesAsync();


                var verifyToken = randomToken;
                var verifyEmail = request.email;

                var queryParams = new Dictionary<string, string>()
                    {
                        {"email", verifyEmail },
                        {"token", verifyToken },
                    };

                var callbackUrl = QueryHelpers.AddQueryString(_resetLink.FrontEndVerifyLink, queryParams);
                await _emailService.SendEmailVerifyUser(callbackUrl, verifyEmail);


                var sendEmail = new EmailDto
                {
                    to = request.email,
                };
                await _emailService.SendEmail(sendEmail, request.email);

                registerResponse.status = true;
                registerResponse.message = "Check email to verify your registration";
                return registerResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                _logger.LogInformation("The error occurred at",
                    DateTime.UtcNow.ToLongTimeString());
                return registerResponse;
            }
        }

        public async Task<LoginViewModel> Login(UserLoginDto request)
        {
            LoginViewModel loginResponse = new LoginViewModel();

            try
            {
                var data = await _context.Users.Include(user => user.UserAccount).FirstOrDefaultAsync(user => user.Username == request.username);
                if (data == null)
                {
                    loginResponse.result = "Invalid Username or Password";
                    _logger.LogWarning("Invalid Username or Password");
                    return loginResponse;
                }

                if (!VerifyPasswordHash(request.password, data.PasswordHash, data.PasswordSalt))
                {
                    loginResponse.result = "Invalid Username or Password";
                    return loginResponse;
                }

                if (data.IsUserLocked == true)
                {
                    loginResponse.result = "Your account has been locked!!! Please contact Admin";
                    return loginResponse;
                }

                string getRole;

                if (request.username.Contains("Admin"))
                {
                    getRole = "Admin";
                }
                else
                {
                    getRole = "User";
                }
                
                string token = CreateToken(data, getRole);

                var refreshToken = GenerateRefreshToken();
                SetRefreshToken(refreshToken);

                RefreshToken addRefreshTokenToDb = new RefreshToken
                {
                    Token = refreshToken.Token,
                    CreatedAt = refreshToken.CreatedAt,
                    ExpiresAt = refreshToken.ExpiresAt,
                    UserId = data.Id
                };

                await _context.RefreshTokens.AddAsync(addRefreshTokenToDb);
                await _context.SaveChangesAsync();

                if (token == null || token == "" || refreshToken == null)
                {
                    loginResponse.result = "Login failed";
                    return loginResponse;
                }

                loginResponse.status = true;
                loginResponse.result = token;
                loginResponse.refreshedToken = refreshToken.Token;
                _logger.LogInformation($"User successfully logged in with {data.Email}");
                return loginResponse;

            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                loginResponse.result = "An exception occured";
                return loginResponse;
            }
        }

        public async Task<AdminLoginViewModel> AdminLogin(AdminLoginDTO request)
        {
            AdminLoginViewModel adminLoginResponse = new AdminLoginViewModel();
            try
            {
                var data = await _context.Admins.FirstOrDefaultAsync(x => x.Username == request.username);

                if (data == null)
                {
                    adminLoginResponse.result = "Invalid Admin Username or Password. Contact Admin if you've made a mistake!!!";
                    _logger.LogWarning("Invalid Admin Username or Password");
                    return adminLoginResponse;
                }

                if (!VerifyPasswordHash(request.password, data.PasswordHash, data.PasswordSalt))
                {
                    adminLoginResponse.result = "Invalid Admin Username or Password";
                    return adminLoginResponse;
                }

                string getRole;

                if (request.username.Contains("Admin"))
                {
                    getRole = "Admin";
                }
                else
                {
                    getRole = "User";
                }

                if (getRole != "Admin")
                {
                    adminLoginResponse.result = "Sorry!!Only an authourized Admin can login";
                    return adminLoginResponse;
                }

                string token = CreateAdminToken(data, getRole);

                var refreshToken = GenerateRefreshToken();
                SetRefreshToken(refreshToken);

                RefreshToken addRefreshTokenToDb = new RefreshToken
                {
                    Token = refreshToken.Token,
                    CreatedAt = refreshToken.CreatedAt,
                    ExpiresAt = refreshToken.ExpiresAt,
                    UserId = data.Id
                };

                await _context.RefreshTokens.AddAsync(addRefreshTokenToDb);
                await _context.SaveChangesAsync();

                if (token == null || token == "" || refreshToken == null)
                {
                    adminLoginResponse.result = "Login failed";
                    return adminLoginResponse;
                }

                adminLoginResponse.status = true;
                adminLoginResponse.isAdmin = true;
                adminLoginResponse.result = token;
                adminLoginResponse.adminUsername = data.Username;
                adminLoginResponse.adminRefreshedToken = refreshToken.Token;
                _logger.LogInformation($"Admin successfully logged in with {data.Username}");
                return adminLoginResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                adminLoginResponse.result = "An exception occured";
                return adminLoginResponse;
            }
        }

        public async Task<VerifyEmailModel> VerifyEmail(VerifyEmailDto verifyReq)
        {
            VerifyEmailModel verifyEmailModel = new VerifyEmailModel();
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == verifyReq.email && u.VerificationToken == verifyReq.token);

                if (user == null)
                {
                    verifyEmailModel.message = "User is not registered";
                    return verifyEmailModel;
                }

                user.VerifiedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                verifyEmailModel.status = true;
                verifyEmailModel.message = "User verified! :)";
                return verifyEmailModel;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return verifyEmailModel;
            }
        }

        public async Task<ChangePasswordModel> ChangePassword(ChangePasswordDto request)
        {
            ChangePasswordModel changePasswordModel = new ChangePasswordModel();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return changePasswordModel;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userID);

                var userSecurityQuest = await _context.SecurityQuestions.FirstOrDefaultAsync(u => u.UserId == userID);

                int tries = userSecurityQuest.Attempts;
                int failedTries;

                if (!(userSecurityQuest.Answer == request.answer))
                {
                    tries--;
                    failedTries = tries--;

                    userSecurityQuest.Attempts = failedTries;
                    await _context.SaveChangesAsync();

                    if (failedTries == 0)
                    {
                        user.IsUserLocked = true;
                        await _context.SaveChangesAsync();

                        changePasswordModel.isLocked = true;
                        changePasswordModel.message = $"You have exhausted all attempts. " +
                            $"You will be redirected to the login page. Please contact Admin for help!!!";
                        return changePasswordModel;
                    }

                    changePasswordModel.isLocked = false;
                    changePasswordModel.message = $"Wrong answer to security question!! You have {failedTries} tries left";
                    return changePasswordModel;
                }

                if (!request.password.Equals(request.confirmPassword))
                {
                    changePasswordModel.message = "Passwords do not match";
                    return changePasswordModel;
                }

                CreatePasswordHash(request.password, out byte[] passwordHash, out byte[] passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;

                await _context.SaveChangesAsync();

                changePasswordModel.status = true;
                changePasswordModel.isLocked = false;
                changePasswordModel.message = "Password changed!";
                return changePasswordModel;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return changePasswordModel;
            }
        }

        public async Task<CreatePinViewModel> CreatePin(CreatePinDto request)
        {
            CreatePinViewModel createPinViewModel = new CreatePinViewModel();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return createPinViewModel;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var userLoggedIn = await _context.Users.Where(u => u.Id == userID).FirstOrDefaultAsync();

                var userSecurityQuest = await _context.SecurityQuestions.Where(s => s.UserId == userID).FirstOrDefaultAsync();

                if (userLoggedIn == null) return createPinViewModel;

                if (userSecurityQuest == null)
                {
                    createPinViewModel.message = "Please set a security question first";
                    return createPinViewModel;
                }

                if (!request.pin.Equals(request.confirmPin))
                {
                    createPinViewModel.message = "Pins do not match";
                    return createPinViewModel;
                }

                CreatePinHash(request.pin, out byte[] pinHash, out byte[] pinSalt);

                userLoggedIn.PinHash = pinHash;
                userLoggedIn.PinSalt = pinSalt;

                await _context.SaveChangesAsync();

                createPinViewModel.status = true;
                createPinViewModel.message = "Pin created successfully";
                return createPinViewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return createPinViewModel;
            }
        }

        public async Task<UpdatePinViewModel> UpdatePin(UpdatePinDto request)
        {
            UpdatePinViewModel updatePinViewModel = new UpdatePinViewModel();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return updatePinViewModel;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var updatedUserProfile = await _context.Users
                    .Where(uProfile => uProfile.Id == userID)
                    .FirstOrDefaultAsync();

                var userSecurityQuest = await _context.SecurityQuestions.Where(s => s.UserId == userID).FirstOrDefaultAsync();

                if (updatedUserProfile == null) return updatePinViewModel;

                int tries = userSecurityQuest.Attempts;
                int failedTries;

                if (!(userSecurityQuest.Answer == request.answer))
                {
                    tries--;
                    failedTries = tries--;

                    userSecurityQuest.Attempts = failedTries;
                    await _context.SaveChangesAsync();

                    if (failedTries == 0)
                    {
                        updatedUserProfile.IsUserLocked = true;
                        await _context.SaveChangesAsync();

                        updatePinViewModel.isLocked = true;
                        updatePinViewModel.message = $"You have exhausted all attempts. " +
                            $"You will be redirected to the login page. Please contact Admin for help!!!";
                        return updatePinViewModel;
                    }

                    updatePinViewModel.isLocked = false;
                    updatePinViewModel.message = $"Wrong answer to security question!! You have {failedTries} tries left";
                    return updatePinViewModel;
                }

                if (!VerifyPinHash(request.oldPin, updatedUserProfile.PinHash, updatedUserProfile.PinSalt))
                {
                    updatePinViewModel.message = "Old Pin does not match pin used during registration";
                    return updatePinViewModel;
                }

                CreatePinHash(request.newPin, out byte[] pinHash, out byte[] pinSalt);

                updatedUserProfile.PinHash = pinHash;
                updatedUserProfile.PinSalt = pinSalt;


                await _context.SaveChangesAsync();

                updatePinViewModel.status = true;
                updatePinViewModel.isLocked = false;
                updatePinViewModel.message = "Pin updated successfully";
                return updatePinViewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                updatePinViewModel.message = "An exception occured";
                return updatePinViewModel;
            }
        }

        private static string CreateRandomToken()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        }

        public async Task<ForgetPasswordModel> ForgetPassword(ForgetPasswordDto emailReq)
        {
            ForgetPasswordModel forgetPassword = new ForgetPasswordModel();
            try
            {
                var userInDb = await _context.Users.FirstOrDefaultAsync(uE => uE.Email == emailReq.email);

                if (userInDb == null)
                {
                    forgetPassword.status = false;
                    forgetPassword.message = "User not found";
                    return forgetPassword;
                }

                var email = userInDb.Email;
                string token = CreateRandomToken();
                userInDb.PasswordResetToken = token;
                userInDb.ResetTokenExpiresAt = DateTime.Now.AddDays(1);

                await _context.SaveChangesAsync();

                //var callbackUrl = _linkGenerator.GetUriByAction("GetResetPassword", "Email", new { token, email }, 
                //    _httpContextAccessor.HttpContext.Request.Scheme, _httpContextAccessor.HttpContext.Request.Host);

                var queryParams = new Dictionary<string, string>()
                {
                    {"email", email },
                    {"token", token },
                };

                var callbackUrl = QueryHelpers.AddQueryString(_resetLink.FrontEndResetLink, queryParams);

                await _emailService.SendEmailPasswordReset(callbackUrl, email);

                if (forgetPassword.status)
                {
                    forgetPassword.status = false;
                    forgetPassword.message = "Email could not be sent";
                    return forgetPassword;
                }

                forgetPassword.status = true;
                forgetPassword.message = $"Forget password link is sent on {email}.";
                return forgetPassword;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return forgetPassword;
            }
        }

        public async Task<ResetPasswordModel> ResetPassword(ResetPasswordDto resetPasswordReq)
        {
            ResetPasswordModel resetPass = new ResetPasswordModel();
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == resetPasswordReq.email && u.PasswordResetToken == resetPasswordReq.token);

                if (user == null)
                {
                    resetPass.message = "Incorrect email/token";
                    return resetPass;
                }

                if (user.ResetTokenExpiresAt < DateTime.Now)
                {
                    resetPass.message = "Token expired. Do Forget Password again!";
                    return resetPass;
                }

                if (!resetPasswordReq.password.Equals(resetPasswordReq.conPassword))
                {
                    resetPass.message = "Passwords do not match";
                    return resetPass;
                }

                CreatePasswordHash(resetPasswordReq.password, out byte[] passwordHash, out byte[] passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
                user.PasswordResetToken = null;
                user.ResetTokenExpiresAt = null;

                await _context.SaveChangesAsync();

                resetPass.status = true;
                resetPass.message = "Password reset done, you can now login!";
                return resetPass;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return resetPass;
            }
        }

        private RefreshTokenViewModel GenerateRefreshToken()
        {
            var refreshToken = new RefreshTokenViewModel
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                ExpiresAt = DateTime.Now.AddDays(7),
                CreatedAt = DateTime.Now
            };

            return refreshToken;
        }

        private void SetRefreshToken(RefreshTokenViewModel newRefreshToken)
        {
            var Response = new HttpResponseMessage();
            var cookieOptions = new CookieOptions();

            cookieOptions.HttpOnly = true;
            cookieOptions.Expires = newRefreshToken.ExpiresAt;

            if (_httpContextAccessor.HttpContext != null)
                _httpContextAccessor.HttpContext.Response.Cookies.Append("refreshToken", newRefreshToken.Token, cookieOptions);

        }

        public async Task<LoginRefreshModel> RefreshToken()
        {
            LoginRefreshModel loginRefreshResponse = new LoginRefreshModel();
            try
            {
                var userData = new User();
                var refreshToken = string.Empty;
                if (_httpContextAccessor.HttpContext != null)
                {
                    refreshToken = _httpContextAccessor.HttpContext.Request.Cookies["refreshToken"];
                    userData.Id = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);
                    userData.FirstName = _httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.FirstName)?.Value;
                    userData.Username = _httpContextAccessor.HttpContext?.User?.FindFirst(CustomClaims.UserName)?.Value;
                }

                var tokenData = await _context.RefreshTokens
                                .OrderByDescending(rt => rt.Token == refreshToken && rt.UserId == userData.Id)
                                .LastOrDefaultAsync();

                if (tokenData == null)
                {
                    loginRefreshResponse.message = "Invalid Refresh Token";
                    return loginRefreshResponse;
                }

                if(tokenData.ExpiresAt < DateTime.Now)
                {
                    loginRefreshResponse.message = "Token Expired";
                    return loginRefreshResponse;
                }

                string getRole;

                if (userData.Username.Contains("Admin"))
                {
                    getRole = "Admin";
                }
                else
                {
                    getRole = "User";
                }

                string token = CreateToken(userData, getRole);

                var newRefreshT = GenerateRefreshToken();
                SetRefreshToken(newRefreshT);

                loginRefreshResponse.status = true;
                loginRefreshResponse.token = token;
                loginRefreshResponse.refreshToken = newRefreshT.Token;
                return loginRefreshResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                loginRefreshResponse.message = "An exception occured";
                return loginRefreshResponse;
            }
        }

        public static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        public static void CreatePinHash(string pin, out byte[] pinHash, out byte[] pinSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                pinSalt = hmac.Key;
                pinHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(pin));
            }
        }

        private string CreateToken(User user, string role)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(CustomClaims.UserId, user.Id.ToString()),
                new Claim(CustomClaims.UserName, user.Username),
                new Claim(CustomClaims.FirstName, user.FirstName),
                new Claim(CustomClaims.Role, role),
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value));

            var credential = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credential);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        private string CreateAdminToken(Admin adminUser, string role)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(AdminCustomClaims.UserName, adminUser.Username),
                new Claim(AdminCustomClaims.Role, role),
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetSection("AdminAppSettings:Token").Value));

            var credential = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credential);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        public static bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        public static bool VerifyPinHash(string pin, byte[] pinHash, byte[] pinSalt)
        {
            if (string.IsNullOrWhiteSpace(pin)) throw new ArgumentNullException("pin");

            using (var hmac = new HMACSHA512(pinSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(pin));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != pinHash[i]) return false;
                }
            }

            return true;
        }
    }
}
