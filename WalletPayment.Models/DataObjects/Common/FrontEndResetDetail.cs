using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletPayment.Models.DataObjects.Common
{
    public class FrontEndResetDetail
    {
        public string FrontEndResetLink { get; set; } = string.Empty;
        public string FrontEndVerifyLink { get; set; } = string.Empty;
    }
}
