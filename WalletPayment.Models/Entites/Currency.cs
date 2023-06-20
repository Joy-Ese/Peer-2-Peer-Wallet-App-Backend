using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletPayment.Models.Entites
{
    public class Currency
    {
        public int Id { get; set; }

        [Column(TypeName = "varchar(10)")]
        public string Currencies { get; set; } = string.Empty;
        public decimal CurrencyCharge { get; set; } = 0;
        public decimal ConversionRate { get; set; } = 0;
    }
}
