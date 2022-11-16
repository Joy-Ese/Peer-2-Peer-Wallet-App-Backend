using System.ComponentModel.DataAnnotations.Schema;

namespace WalletPayment.Models.Entites
{
    public class Transaction
    {
        public int Id { get; set; }
        public string TransactionReference { get; set; }
        public decimal TransactionAmount { get; set; }
        public TranStatus TransactionStatus { get; set; }
        public bool IsSuccessful => TransactionStatus.Equals(TranStatus.Success);

        [Column(TypeName = "varchar(50)")]
        public string TranSourceAccount { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string TranDestinationAccount { get; set; }
        public TranType TransactionType { get; set; }
        public DateTime TransactionDate { get; set; }

        public Transaction()
        {
            TransactionReference = $"{Guid.NewGuid().ToString().Replace("-","").Substring(1,27)}";
        }
    }
    public enum TranStatus
    {
        Failed,
        Success,
        Error
    }
    public enum TranType
    {
        Transfer
    }
}

