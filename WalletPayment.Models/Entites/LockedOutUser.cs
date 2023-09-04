using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletPayment.Models.Entites
{
    public class LockedOutUser
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string SixDigitPin { get; set; } = string.Empty;
        public DateTime PinGenerationTime { get; set; }
        public string? Username { get; set; } = string.Empty;
    }
}
