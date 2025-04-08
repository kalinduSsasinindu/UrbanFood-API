namespace DMCW.API.Dtos.Order
{
    public class OrderPaymentWebDto
    {
        public decimal? SubtotalPrice { get; set; }
        public decimal? TotalLineItemsPrice { get; set; }
        public decimal? TotalPrice { get; set; }
        public decimal? TotalShippingPrice { get; set; }
        public decimal? TotalDiscountPrice { get; set; }
    }
}
