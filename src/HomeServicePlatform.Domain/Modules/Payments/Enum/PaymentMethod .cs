using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Domain.Modules.Payments.Enum
{
    public enum PaymentMethod : short
    {
        Wallet = 1,
        Cash = 2,
        Momo = 3,
        ZaloPay = 4
    }
}
