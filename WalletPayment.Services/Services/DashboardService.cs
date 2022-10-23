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

                var userData = await _context.Users
                                .Where(userInfo => userInfo.Id == userID)
                                .Select(userInfo => new UserDashboardViewModel
                                {
                                    Username = userInfo.Username,
                                    FirstName = userInfo.FirstName,
                                    LastName = userInfo.LastName,
                                    AccountNumber = userInfo.UserAccount.AccountNumber
                                })
                                .FirstOrDefaultAsync();

                if (userData == null) return new UserDashboardViewModel();

                return userData;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new UserDashboardViewModel();
            }
        }
    }
}





