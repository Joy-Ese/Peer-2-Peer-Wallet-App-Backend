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

        public AuthService(DataContext context,IEmail emailService, IConfiguration configuration,
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
            var accountNumber = DateTime.Now.ToString("ffffMMddHH");
            return accountNumber;
        }

        public async Task<UserSignUpDto> Register(UserSignUpDto request)
        {
            RegisterViewModel registerResponse = new RegisterViewModel();
            try
            {
                var data = await _context.Users
                    .AnyAsync(user => user.Username == request.username || user.PhoneNumber == request.phoneNumber || user.Email == request.email);
                
                if (data)
                {
                    _logger.LogWarning($"Duplicate username supplied {request.username}");
                    registerResponse.message = "Duplicate username or phonenumber or email";
                    return null;
                }

                if (!request.pin.Equals(request.confirmPin)) throw new ArgumentException("Pins do not match", "pin");

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
                    Balance = 10000,
                    Currency = "NGN",
                    UserId = newUser.Id
                };

                await _context.Accounts.AddAsync(newAccount);
                var result = await _context.SaveChangesAsync();
                var token = new CancellationToken();

                var sendEmail = new EmailDto
                {
                    to = request.email,
                };
                await _emailService.SendEmail(sendEmail, request.email, token);


                if (!(result > 0)) return null;

                return request;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                _logger.LogInformation("The error occurred at",
                    DateTime.UtcNow.ToLongTimeString());
                return new UserSignUpDto();
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
                    //userData.UserAccount.AccountNumber = _httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.AccountNumber)?.Value;
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

        public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        public void CreatePinHash(string pin, out byte[] pinHash, out byte[] pinSalt)
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
                //new Claim(CustomClaims.AccountNumber, user.UserAccount.AccountNumber),
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value));

            var credential = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: credential);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        public bool VerifyPinHash(string pin, byte[] pinHash, byte[] pinSalt)
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
