using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletPayment.Models.Entites
{
    public class UserToUserChat
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime Date { get; set; }

        public string? SenderUsernmae { get; set; } = string.Empty;

        public string? RecipientUsername { get; set; } = string.Empty;
        public bool IsChatRead { get; set; }




    }
}
