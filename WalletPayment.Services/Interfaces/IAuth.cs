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
        Task<UserSignUpDto> Register(UserSignUpDto request);
        Task<LoginViewModel> Login(UserLoginDto request);
        Task<LoginRefreshModel> RefreshToken();
        public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt);
        public bool VerifyPinHash(string pin, byte[] pinHash, byte[] pinSalt);
        public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt);
        public void CreatePinHash(string pin, out byte[] pinHash, out byte[] pinSalt);
    }
}
