using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMCW.Service.Helper
{
    public enum PaymentOptions
    {
        [Description("Card Payments")]
        CardPayments = 1,
        [Description("Cash On Delivery")]
        COD = 2,
        [Description("Bank Transfer")]
        BankTransfer = 3,
        [Description("KOKO Pay")]
        KOKO = 4,
        [Description("Mint Pay")]
        MintPay = 5,
        [Description("Bee Wallet")]
        BeeWallet = 6
    }
}
