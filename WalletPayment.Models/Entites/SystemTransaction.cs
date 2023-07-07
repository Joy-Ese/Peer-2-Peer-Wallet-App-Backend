using System.ComponentModel.DataAnnotations.Schema;

namespace WalletPayment.Models.Entites
{
    public class SystemTransaction
    {
        public int Id { get; set; }
        public string Reference { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Narration { get; set; } = string.Empty;

        [Column(TypeName = "varchar(50)")]
        public string WalletUserAccount { get; set; }
        public DateTime Date { get; set; }
        public int WalletAccountUserId { get; set; }

        [ForeignKey("WalletAccountUserId")]
        public virtual User WalletUser { get; set; }




        public SystemTransaction()
        {
            Reference = $"{Guid.NewGuid().ToString().Replace("-", "").Substring(1, 15)}";
        }
    }
}
