

using System.ComponentModel.DataAnnotations.Schema;

namespace WalletPayment.Models.Entites
{
    public class SystemAccount
    {
        public int Id { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string SystemAccountNumber { get; set; } = string.Empty;

        public decimal SystemBalance { get; set; } = 0;

        [Column(TypeName = "varchar(10)")]
        public string Currency { get; set; } = string.Empty;
    }
}
