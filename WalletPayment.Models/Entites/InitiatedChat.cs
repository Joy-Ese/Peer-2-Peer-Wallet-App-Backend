using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletPayment.Models.Entites
{
    public class InitiatedChat
    {
        public int Id { get; set; }
        public bool IsChatInitiated { get; set; }
        public string StartedWith { get; set; } = string.Empty;
        public string ReceivedFrom { get; set; } = string.Empty;
    }
}
