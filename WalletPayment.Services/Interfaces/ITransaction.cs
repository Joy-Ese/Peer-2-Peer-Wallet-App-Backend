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
        Task<Response> FindTransactionByDate(DateTime date);
        Task<Response> TransferFund(TransactionDto request);



    }
}





