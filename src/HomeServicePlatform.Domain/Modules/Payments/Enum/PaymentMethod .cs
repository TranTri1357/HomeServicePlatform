using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Domain.Modules.Payments.Enum
{
    public enum PaymentMethod : short
    {
        Wallet = 1,      // Ví trong hệ thống
        Cash = 2,        // Tiền mặt
        Momo = 3,
        ZaloPay = 4
    }
}
