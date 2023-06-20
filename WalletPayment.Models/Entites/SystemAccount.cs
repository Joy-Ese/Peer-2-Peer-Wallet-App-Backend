

using System.ComponentModel.DataAnnotations.Schema;

namespace WalletPayment.Models.Entites
{
    public class SystemAccount
    {
        public int Id { get; set; }

        public decimal SystemBalance { get; set; } = 0;

        [Column(TypeName = "varchar(10)")]
        public string Currency { get; set; } = string.Empty;
    }
}
