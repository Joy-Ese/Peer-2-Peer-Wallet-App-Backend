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
        Task<AdminViewModel> CreateAdmin(CreateAdminDTO request);
        Task<RegisterViewModel> Register(UserSignUpDto request);
        Task<LoginViewModel> Login(UserLoginDto request);
        Task<AdminLoginViewModel> AdminLogin(AdminLoginDTO request);
        Task<ResponseModel> ChangeAdminPassword(ChangeAdminPasswordDTO req);
        Task<CreatePinViewModel> CreatePin(CreatePinDto request);
        Task<UpdatePinViewModel> UpdatePin(UpdatePinDto request);
        Task<ForgetPasswordModel> ForgetPassword(ForgetPasswordDto emailReq);
        Task<ResetPasswordModel> ResetPassword(ResetPasswordDto resetPasswordReq);
        Task<VerifyEmailModel> VerifyEmail(VerifyEmailDto verifyReq);
        Task<ChangePasswordModel> ChangePassword(ChangePasswordDto request);
        Task<LoginRefreshModel> RefreshToken();
    }
}
