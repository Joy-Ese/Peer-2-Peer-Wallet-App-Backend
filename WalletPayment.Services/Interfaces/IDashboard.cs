using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletPayment.Models.DataObjects;

namespace WalletPayment.Services.Interfaces
{
    public interface IDashboard
    {
        Task<UserDashboardViewModel> GetUserDetails();

        Task<UserBalanceViewModel> GetUserAccountBalance();

        Task<UserDashboardViewModel> GetUserAccountNumber();

        Task<UserUpdateViewModel> UpdateUserProfile(UserUpdateDto request);
    }
}

