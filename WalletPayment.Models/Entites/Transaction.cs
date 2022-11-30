using System.ComponentModel.DataAnnotations.Schema;

namespace WalletPayment.Models.Entites
{
    public class Transaction
    {
        public int Id { get; set; }
        public string TransactionReference { get; set; }
        public decimal TransactionAmount { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string TranSourceAccount { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string TranDestinationAccount { get; set; }
        public DateTime TransactionDate { get; set; }

        public Transaction()
        {
            TransactionReference = $"{Guid.NewGuid().ToString().Replace("-","").Substring(1,27)}";
        }
    }
    
}

