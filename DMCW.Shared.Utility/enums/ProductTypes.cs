using System.ComponentModel;

namespace DMCW.Shared.Utility.Enums
{
    public enum ProductType
    {

        [Description("Fruit")]
        Fruit = 1,

        [Description("Vegetable")]
        Vegetable = 2,

        [Description("Dairy Product")]
        DairyProduct = 3,

        [Description("Baked Good")]
        BakedGood = 4,

        [Description("Handmade Craft")]
        HandmadeCraft = 5
    }
}
