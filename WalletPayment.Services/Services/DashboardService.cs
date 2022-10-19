using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletPayment.Models.DataObjects;
using WalletPayment.Services.Data;
using WalletPayment.Services.Interfaces;

namespace WalletPayment.Services.Services
{
    public class DashboardService : IDashboard
    {
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DashboardService(DataContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> GetUserDetails()
        {
            try
            {
                string userID = string.Empty;
                if(_httpContextAccessor.HttpContext != null)
                {
                    userID = _httpContextAccessor.HttpContext.User.FindFirst(CustomClaims.UserId).Value;
                }


                var userDATA = await _context.Users
                                .Where(userInfo => userInfo.Id.ToString() == userID)
                                .Select(userInfo => new 
                                { 
                                    userInfo.Username, userInfo.FirstName, userInfo.LastName, userInfo.UserAccount.AccountNumber 
                                })
                                .FirstOrDefaultAsync();



                return userDATA.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "Error Occured";
            }
        }
    }
}





