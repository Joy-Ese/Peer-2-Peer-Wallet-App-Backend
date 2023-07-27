using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletPayment.Models.Entites
{
    public class KycImage
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public DateTime TimeUploaded { get; set; }
        public bool IsAccepted { get; set; }
        public bool IsRejected { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User User { get; set; }
    }
}
