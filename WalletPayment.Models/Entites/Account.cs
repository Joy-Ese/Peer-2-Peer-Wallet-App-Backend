using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WalletPayment.Models.Entites
{
    public class Account
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; } = string.Empty;

        public decimal Balance { get; set; } = 0;

        public string Currency { get; set; } = string.Empty;

        [ForeignKey("User")]
        public int UserId { get; set; }


        public virtual User User { get; set; }
    }
}
