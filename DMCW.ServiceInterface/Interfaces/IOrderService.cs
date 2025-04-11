using DMCW.Repository.Data.Entities.Order;
using DMCW.Repository.Data.Entities.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMCW.ServiceInterface.Interfaces
{
    public interface IOrderService
    {
        // Get/Search operations
        Task<IEnumerable<OrderSearchResponse>> GetOrders();
        Task<IEnumerable<OrderSearchResponse>> GetOrdersByStatus(string fulfilmentStatus);
        Task<List<Order>> GetByIds(List<string> ids);
        Task<Order> GetById(string id);
        Task<List<OrderSearchResponse>> SearchOrdersAsync(string? query);
        
        // Seller-specific operations
        Task<List<OrderSearchResponse>> GetOrdersBySellerId(string sellerId);
        Task<List<Order>> GetOrdersWithLineItemsBySellerId(string sellerId);
        Task<List<Order>> GetOrderGroupedBySeller(string orderId);

        // Create operations
        Task Create(Order order);

        // Update operations
        Task Update(string id, List<LineItem> lineItems);
        Task Update(string id, ShippingAddress shippingAddress);
        Task Update(string id, PaymentInfo paymentInfo);
        Task<bool> Update(string id, decimal subtotalPrice, decimal totalLineItemsPrice, decimal totalPrice, decimal totalShippingPrice, decimal totalDiscountPrice);
        
        // Status update operations
        Task UpdateFulfillStatus(string id, string fulfillStatus);
        Task UpdatePaymentStatus(string id, string paymentStatus, decimal amount = 0, string paymentMethod = null);
        
        // Timeline operations
        Task AddTimelineAsync(string orderId, TimeLineDetails details);
        
        // Delete/Cancel operations
        Task<bool> DeleteProductAsync(string id);
        Task<bool> CancelOrderAsync(string id);
        
        // Tag operations
        Task AddTagToOrder(string orderId, List<string> tagNames);
    }
}
