using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletPayment.Models.DataObjects.Common
{
    public class PaystackDetails
    {
        public PaystackDetails()
        {
            WhitelistedPaystackIPs = new List<string>();
        }

        public string PaystackInitializeApi { get; set; } = string.Empty;
        public List<String> WhitelistedPaystackIPs { get; set; }
        public string PaystackVerifyApi { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
    }
}
