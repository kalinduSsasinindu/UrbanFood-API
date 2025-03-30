

using System.ComponentModel;

namespace DMCW.Shared.Utility.enums
{
    public enum SearchFilterEntityTypes
    {

        [Description("Customer")]
        Customer = 1,
        [Description("Order")]
        Order = 2,
        [Description("Product")]
        Product = 3,
        [Description("TrackingInformation")]
        TrackingInformation = 4,
        [Description("Sequence")]
        Sequence = 5,
        [Description("Cities")]
        Cities = 6,
        [Description("User")]
        User = 7,

        [Description("Tag")]
        Tag = 9,
    
        [Description("SecurityRole")]
        SecurityRole = 13,
      
        [Description("SearchFilter")]
        SearchFilter = 18,
      
    }
}
