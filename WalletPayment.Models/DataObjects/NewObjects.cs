using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletPayment.Models.DataObjects
{
    public class RespModel
    {
        public bool status { get; set; }
        public string message { get; set; } = string.Empty;
    }

    public class RespnModel<T>
    {
        public bool status { get; set; }
        public string message { get; set; } = string.Empty;
        public T result { get; set; }
    }

    public class SearchContactModel
    {
        public string firstName { get; set; } = string.Empty;
        public string lastName { get; set; } = string.Empty;
        public string userName { get; set; } = string.Empty;
        public byte[] imageDetails { get; set; }
    }

    public class GenratePinDTO
    {
        public string uName { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public bool showInputBox { get; set; }
    }

    public class ValidatePinDTO
    {
        public string uName { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public bool showInputBox { get; set; }
        public string pin { get; set; } = string.Empty;
    }

    public class ChatTrnModel
    {
        public string sourceAccount { get; set; } = string.Empty;
        public string destinationAccount { get; set; } = string.Empty;
    }

    public class InitiatedChats
    {
        public string startedWith { get; set; } = string.Empty;
        public string receivedFrom { get; set; } = string.Empty;
    }

    public class UnreadChats
    {
        public string senderUsername { get; set; } = string.Empty;
        public int unreadChat { get; set; }
    }




}
