using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletPayment.Models.DataObjects;
using WalletPayment.Models.Entites;

namespace WalletPayment.Services.Interfaces
{
    public interface IAccount
    {
        Task<UpdateChargeOrRateModel> UpdateChargeOrRate(UpdateChargeOrRateDTO req);
        Task<CreateSystemAccountsModel> CreateSystemAccount(CreateSystemAccountsDTO req);
        Task<Account> GetByAccountNumber(string AccountNumber);
        Task<AccountViewModel> AccountLookUp(string searchInfo, string currency);
        Task<SendMoneyCheckModel> SendMoneyCheck(string currency);
        Task<CreateWalletModel> CreateForeignWallet(CreateWalletDTO req);
        Task<List<AvailableCurrenciesModel>> CurrenciesSeededInDb();
        Task<List<AvailableCurrenciesModel>> UnavailableCurrencies();
        Task<List<AvailableCurrenciesModel>> FundWalletCurrencies();
        Task<List<AccountDetails>> UserAccountDetails();
        Task<CurrencyChargeModel> GetCurrencyCharges();
        Task<CurrencyChargeModel> GetConversionRates();
        Task<NairaBalModel> GetNairaBalance();
        Task<FundWalletModel> FundForeignWallet(FundWalletDTO req);
        Task<UserDataForAdminModel> GetUserDataForAdmin();
        Task<List<SystemAccountDetails>> SystemAccountDetails();
        Task<LockOrUnlockUsersModel> LockOrUnlockUsers(LockOrUnlockUsersDTO req);
        Task<List<LockedUsersListModel>> GetLockedUsersList();
        Task<IsLoggedInModel> GetAdminIsLoggedIn();
        Task<List<ReturnedModel>> GetUserIsLoggedIn();
        Task<bool> IsUserLoggedIn();
        
    }
}



