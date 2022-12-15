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
        public bool status { get; set; }
        public string responseMessage { get; set; }
        public object Data { get; set; }
    }
}
