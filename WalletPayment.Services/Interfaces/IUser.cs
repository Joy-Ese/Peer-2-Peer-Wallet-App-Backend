using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletPayment.Models.DataObjects;
using WalletPayment.Models.Entites;

namespace WalletPayment.Services.Interfaces
{
    public interface IUser
    {
        Task<UserSignUpDto> Register(UserSignUpDto request);
        Task<LoginViewModel> Login(UserLoginDto request);
        Task<AccountViewModel> AccountLookUp(string AccountNumber);
    }
}
