using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletPayment.Models.DataObjects;
using WalletPayment.Models.Entites;

namespace WalletPayment.Services.Interfaces
{
    public interface IAuth
    {
        Task<RegisterViewModel> Register(UserSignUpDto request);
        Task<LoginViewModel> Login(UserLoginDto request);
        Task<LoginRefreshModel> RefreshToken();
    }
}
