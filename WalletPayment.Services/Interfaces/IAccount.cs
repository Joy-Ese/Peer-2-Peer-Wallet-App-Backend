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
        Task<Account> GetByAccountNumber(string AccountNumber);
        Task<AccountViewModel> AccountLookUp(string searchInfo);
        Task<CreateWalletModel> CreateForeignWallet(CreateWalletDTO req);
        Task<List<AvailableCurrenciesModel>> UnavailableCurrencies();
        Task<List<AccountDetails>> UserAccountDetails();
        Task<CurrencyChargeModel> GetCurrencyCharges();
    }
}



