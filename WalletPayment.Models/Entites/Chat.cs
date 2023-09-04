using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletPayment.Models.Entites
{
    public class Chat
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int? UserId { get; set; }
        public virtual User User { get; set; }

        public int? AdminUserId { get; set; }
        public virtual Admins AdminUser { get; set; }
        public bool IsChatRead { get; set; }
        public string ChattingWith { get; set; } = string.Empty;
    }
}
