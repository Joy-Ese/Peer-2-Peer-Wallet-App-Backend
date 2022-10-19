using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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

        public UserService(DataContext context, IConfiguration configuration, 
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
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
                if (data) return null;

                CreatePasswordHash(request.password, out byte[] passwordHash, out byte[] passwordSalt);
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
                    Address = request.address
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
                Console.WriteLine(ex.Message);
                return new UserSignUpDto();
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
                Console.WriteLine(ex.Message);
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

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(CustomClaims.UserId, user.Id.ToString()),
                new Claim(CustomClaims.UserName, user.Username),
                new Claim(CustomClaims.FirstName, user.FirstName),
                new Claim(CustomClaims.AccountNumber, user.UserAccount.AccountNumber),
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
    }
}
