using Microsoft.AspNetCore.Authentication.JwtBearer;
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

namespace WalletPayment.Services.Services
{
    public class AuthService : IAuth
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmail _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuthService> _logger;

        public AuthService(DataContext context, IEmail emailService, IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor, ILogger<AuthService> logger)
        {
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _logger.LogDebug(1, "Nlog injected into AuthService");
        }

        public string AccountNumberGenerator()
        {
            var accountNumber = DateTime.Now.ToString("HHffMMffdd");
            return accountNumber;
        }

        public string GenerateSystemAcctNumber()
        {
            Random rnd = new Random(10);
            var sysAcctNumber = rnd.Next().ToString();
            return sysAcctNumber;
        }

        public async Task<SystemAccount> CreateSystemAccount()
        {
            string generatedSysAcct = GenerateSystemAcctNumber();

            SystemAccount sysAccount = new SystemAccount
            {
                SystemAccountNumber = generatedSysAcct,
                SystemBalance = 0,
                Currency = "NGN",
            };

            await _context.SystemAccounts.AddAsync(sysAccount);
            await _context.SaveChangesAsync();

            return sysAccount;
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
                    _logger.LogWarning($"Duplicate username supplied {request.username}");
                    registerResponse.message = "Duplicate username or email";
                    return registerResponse;
                }

                if (!request.pin.Equals(request.confirmPin))
                {
                    registerResponse.message = "Pins do not match";
                    return registerResponse;
                }

                CreatePasswordHash(request.password, out byte[] passwordHash, out byte[] passwordSalt);

                CreatePinHash(request.pin, out byte[] pinHash, out byte[] pinSalt);

                string generatedAcc = AccountNumberGenerator();

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
                    PinHash = pinHash,
                    PinSalt = pinSalt
                };
                var userResult = await _context.Users.AddAsync(newUser);
                var result1 = await _context.SaveChangesAsync();
                
                Account newAccount = new Account
                {
                    AccountNumber = generatedAcc,
                    Balance = 0,
                    Currency = "NGN",
                    UserId = newUser.Id
                };

                await _context.Accounts.AddAsync(newAccount);
                var result = await _context.SaveChangesAsync();

                var sendEmail = new EmailDto
                {
                    to = request.email,
                };
                await _emailService.SendEmail(sendEmail, request.email);


                if (!(result > 0))
                {
                    registerResponse.message = "Duplicate username or email";
                }

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
                    return loginResponse;
                }

                if (!VerifyPasswordHash(request.password, data.PasswordHash, data.PasswordSalt))
                {
                    loginResponse.result = "Invalid Username or Password";
                    return loginResponse;
                }

                string token = CreateToken(data);

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
                return loginResponse;

            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                loginResponse.result = "An exception occured";
                return loginResponse;
            }
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

                //var callbackUrl = _linkGenerator.GetUriByAction("GetResetPassword", "Email", new { token, email }, 
                //    _httpContextAccessor.HttpContext.Request.Scheme, _httpContextAccessor.HttpContext.Request.Host);

                var callbackUrl = "http://127.0.0.1:5500/html/ResetPassword.html";

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
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == resetPasswordReq.email);

                if (user == null)
                {
                    resetPass.message = "Incorrect email";
                    return resetPass;
                }

                CreatePasswordHash(resetPasswordReq.password, out byte[] passwordHash, out byte[] passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;

                await _context.SaveChangesAsync();

                resetPass.status = true;
                resetPass.message = "Password sucsessfully reset, you can now login!";
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

                string token = CreateToken(userData);
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

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(CustomClaims.UserId, user.Id.ToString()),
                new Claim(CustomClaims.UserName, user.Username),
                new Claim(CustomClaims.FirstName, user.FirstName),
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
