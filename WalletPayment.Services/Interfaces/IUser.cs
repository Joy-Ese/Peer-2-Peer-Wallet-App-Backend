using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletPayment.Models.DataObjects;

namespace WalletPayment.Services.Interfaces
{
    public interface IUser
    {
        Task<UserSignUpDto> Register(UserSignUpDto request);
        Task<string> Login(UserLoginDto request);
    }
}
