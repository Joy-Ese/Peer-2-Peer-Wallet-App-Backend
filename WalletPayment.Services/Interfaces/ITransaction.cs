using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletPayment.Models.DataObjects;
using WalletPayment.Models.Entites;

namespace WalletPayment.Services.Interfaces
{
    public interface ITransaction
    {
        Task<TransactionResponseModel> TransferFund(TransactionDto request);
        Task<List<TransactionListModel>> GetTransactionList();
        Task<List<TransactionListModel>> GetLastThreeTransactions();
        Task<List<TransactionListModel>> TransactionsByDateRange(TransactionDateDto request);
    }
}





