using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletPayment.Models.Entites
{
    public enum StatusMessage
    {
        Successful = 00,
        Pending = 01,
        Failed = 99,
    }

    public class Deposit
    {
        public int Id { get; set; }

        [Column(TypeName = "varchar(10)")]
        public string Status { get; set; } = string.Empty;

        [Column(TypeName = "varchar(30)")]
        public string Reference { get; set; } = string.Empty;
        public decimal Amount { get; set; }

        [Column(TypeName = "varchar(10)")]
        public string Currency { get; set; } = string.Empty;
        public DateTime Date { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User User { get; set; }
    }

}
