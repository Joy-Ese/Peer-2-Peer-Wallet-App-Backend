using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletPayment.Models.DataObjects.Common
{
    public class EmailCredentials
    {
        public string EmailUsername { get; set; } = string.Empty;
        public string EmailPassword { get; set; } = string.Empty;
        public string EmailHost { get; set; } = string.Empty;
        public string EmailFrom { get; set; } = string.Empty;
        public bool enableSSL { get; set; }
    }
}
