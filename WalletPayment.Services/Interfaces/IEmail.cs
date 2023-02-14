﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletPayment.Models.DataObjects;

namespace WalletPayment.Services.Interfaces
{
    public interface IEmail
    {
        Task<bool> SendEmail(EmailDto request,string emailUser);
        Task<bool> SendCreditEmail(EmailDto request, string emailUser);
        Task<bool> SendDebitEmail(EmailDto request,string emailUser);
    }
}


