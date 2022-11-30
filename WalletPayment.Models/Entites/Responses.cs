using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletPayment.Models.Entites
{
    public class Responses
    {
        public string RequestId => $"{Guid.NewGuid().ToString()}";
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public object Data { get; set; }
    }
}
