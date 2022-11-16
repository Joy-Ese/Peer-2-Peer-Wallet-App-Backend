﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
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
    public class UserService : IUser
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UserService> _logger;

        public UserService(DataContext context, IConfiguration configuration, 
            IHttpContextAccessor httpContextAccessor, ILogger<UserService> logger)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _logger.LogDebug(1, "Nlog injected into UserService");
        }

        public string AccountNumberGenerator()
        {
            var accountNumber = DateTime.Now.ToString("ffffMMddHH");
            return accountNumber;
        }

        public async Task<UserSignUpDto> Register(UserSignUpDto request)
        {
            try
            {
                var data = await _context.Users.AnyAsync(user => user.Username == request.username);
                if (data)
                {
                    _logger.LogWarning($"Duplicate username supplied {request.username}");
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

        public async Task<AccountViewModel> Authenticate(string AccountNumber)
        {
            AccountViewModel result = new AccountViewModel();
            try
            {
                var userData = await _context.Users.Where(userAcc => userAcc.UserAccount.AccountNumber == AccountNumber).SingleOrDefaultAsync();
                if (userData == null)
                    return result;

                result.FirstName = userData.FirstName;
                result.LastName = userData.LastName;

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                _logger.LogInformation("The error occurred at",
                    DateTime.UtcNow.ToLongTimeString());
                return result;
            }
        }



        public async Task<string> Login(UserLoginDto request)
        {
            try
            {
                var data = await _context.Users.Include(user => user.UserAccount).FirstOrDefaultAsync(user => user.Username == request.username);
                if (data == null) return "Invalid Username or Password";

                if (!VerifyPasswordHash(request.password, data.PasswordHash, data.PasswordSalt))
                {
                    return "Invalid Username or Password";
                }

                string token = CreateToken(data);

                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                _logger.LogInformation("The error occurred at",
                    DateTime.UtcNow.ToLongTimeString());
                return string.Empty;
            }
        } 

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private void CreatePinHash(string pin, out byte[] pinHash, out byte[] pinSalt)
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
                new Claim(CustomClaims.AccountNumber, user.UserAccount.AccountNumber),
                new Claim(CustomClaims.Balance, user.UserAccount.Balance.ToString()),
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

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        private bool VerifyPinHash(string pin, byte[] pinHash, byte[] pinSalt)
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
