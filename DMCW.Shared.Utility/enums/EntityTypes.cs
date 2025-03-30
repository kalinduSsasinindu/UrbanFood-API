
using System.ComponentModel;


namespace DMCW.Shared.Utility.enums
{
    public enum EntityTypes
    {
        [Description("Customer")]
        Customer = 1,
        [Description("Order")]
        Order = 2,
        [Description("Product")]
        Product = 3,
        [Description("Sequence")]
        Sequence = 4,
        [Description("Cities")]
        Cities = 6,
        [Description("User")]
        User = 7,    
        [Description("PaymentMethod")]
        PaymentMethod = 20,
        [Description("TemporaryOrder")]
        TemporaryOrder = 21
    }
}
