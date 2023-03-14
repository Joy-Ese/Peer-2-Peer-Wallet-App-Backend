using System.ComponentModel.DataAnnotations.Schema;

namespace WalletPayment.Models.Entites
{
    public class Transaction
    {
        public int Id { get; set; }
        public string Reference { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string TranSourceAccount { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string TranDestinationAccount { get; set; }

        public DateTime Date { get; set; }
        public int? SourceAccountUserId { get; set; }
        public int? DestinationAccountUserId { get; set; }

        [ForeignKey("SourceAccountUserId")]
        [InverseProperty("Transactions")]
        public virtual User SourceUser { get; set; }

        [ForeignKey("DestinationAccountUserId")]
        public virtual User DestinationUser { get; set; }

        public Transaction()
        {
            Reference = $"{Guid.NewGuid().ToString().Replace("-","").Substring(1,15)}";
        }
    }
    
}

