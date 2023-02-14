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
    
    public class PaystackDeposit
    {
        public int Id { get; set; }
        public StatusMessage StatusMessage { get; set; }
        public string PaystackDepositStatus { get; set; } = string.Empty;
        public string PaystackReference { get; set; } = string.Empty;
        public string PaystackEmail { get; set; } = string.Empty;
        public decimal PaystackAmount { get; set; }
        public string PaystackCurrency { get; set; } = string.Empty;
        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User User { get; set; }
    }

}
