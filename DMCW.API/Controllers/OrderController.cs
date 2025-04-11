using AutoMapper;
using CloudinaryDotNet.Actions;
using DMCW.API.Dtos.Order;
using DMCW.Repository.Data.DataService;
using DMCW.Repository.Data.Entities.Order;
using DMCW.Repository.Data.Entities.Search;
using DMCW.ServiceInterface.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace DMCW.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly ILogger<OrderController> _logger;
        private readonly IOrderService _orderService;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly MongoDBContext _context;

        public OrderController(
            ILogger<OrderController> logger, 
            IOrderService orderService, 
            IMapper mapper, 
            IUserService userService,
            MongoDBContext context)
        {
            _logger = logger;
            _orderService = orderService;
            _mapper = mapper;
            _userService = userService;
            _context = context;
        }

        [Authorize]
        [HttpGet]
        public async Task<IEnumerable<OrderSearchResponse>> GetOrders()
        {
            var filteredRes = await _orderService.GetOrders();
            return filteredRes;
        }

        [Authorize]
        [HttpGet("GetOrdersByStatus/{fulfillmentStatus}")]
        public async Task<IEnumerable<OrderSearchResponse>> GetOrdersByStatus(string fulfillmentStatus)
        {
            var filteredRes = await _orderService.GetOrdersByStatus(fulfillmentStatus);
            return filteredRes;
        }




       
        [HttpPost]
        public async Task<ActionResult> create(Order order)
        {
            await _orderService.Create(order);
            await _orderService.AddTimelineAsync(order.Id, new TimeLineDetails
            {
                CreatedAt = DateTime.Now,
                Comment = "Order placed successfully"
            });
            return Ok(new { id = order.Id.ToString() });
        }

        [Authorize]
        [HttpPatch("{id}/timeline")]
        public async Task<ActionResult> Update(string id, TimeLineDetails timeLineDetails)
        {
            await _orderService.AddTimelineAsync(id, timeLineDetails);
            return Ok();
        }

        [Authorize]
        [HttpPatch("{id}/lineItems")]
        public async Task<ActionResult> Update(string id, List<LineItem> lineItems)
        {
            await _orderService.Update(id, lineItems);
            return Ok();
        }

        [Authorize]
        [HttpPatch("{id}/shippingAddress")]
        public async Task<ActionResult> Update(string id, ShippingAddress shippingAddress)
        {
            await _orderService.Update(id, shippingAddress);
            return Ok();
        }

        [Authorize]
        [HttpPatch("{id}/paymentInfo")]
        public async Task<ActionResult> Update(string id, PaymentInfo paymentInfo)
        {
            await _orderService.Update(id, paymentInfo);
            return Ok();
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetById(string id)
        {
            var order = await _orderService.GetById(id);
            return order is not null ? Ok(order) : NotFound();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            var success = await _orderService.DeleteProductAsync(id);
            return success ? Ok() : NotFound();
        }

        [Authorize]
        [HttpPut("{id}/cancel")]
        public async Task<ActionResult> Cancel(string id)
        {
            var success = await _orderService.CancelOrderAsync(id);
            return success ? Ok() : NotFound();
        }

        [Authorize]
        [HttpPatch("{id}/paymentAmounts")]
        public async Task<ActionResult> Update(string id, [FromBody] OrderPaymentWebDto updateDto)
        {
            if (updateDto == null)
            {
                return BadRequest("Update data is required.");
            }
            var existingOrder = await _orderService.GetById(id);
            if (existingOrder == null)
            {
                return NotFound("Order not found.");
            }

            var subtotalPrice = updateDto.SubtotalPrice ?? existingOrder.SubtotalPrice;
            var totalLineItemsPrice = updateDto.TotalLineItemsPrice ?? existingOrder.TotalLineItemsPrice;
            var totalPrice = updateDto.TotalPrice ?? existingOrder.TotalPrice;
            var totalShippingPrice = updateDto.TotalShippingPrice ?? existingOrder.TotalShippingPrice;
            var totalDiscountPrice = updateDto.TotalDiscountPrice ?? existingOrder.TotalDiscountPrice;

            await _orderService.Update(id, subtotalPrice, totalLineItemsPrice, totalPrice, totalShippingPrice, totalDiscountPrice);

            return Ok();
        }

       
        [HttpGet("order")]
        public async Task<ActionResult<List<OrderSearchResponse>>> SearchOrder([FromQuery] string? query)
        {
            var result = await _orderService.SearchOrdersAsync(query);
            return Ok(result);
        }


        [HttpPost("{orderId}/add-tag")]
        public async Task<IActionResult> AddTagToOrder(string orderId, List<string> tagNames)
        {
            await _orderService.AddTagToOrder(orderId, tagNames);
            return Ok();
        }

        [Authorize]
        [HttpGet("seller")]
        public async Task<ActionResult<List<OrderSearchResponse>>> GetSellerOrders()
        {
            try
            {
                // Get current user information
                var user = await _userService.GetUserByEmail();
                
                // Check if user is a seller
                if (user.UserRole != "Seller")
                {
                    return Forbid("Only sellers can access seller orders");
                }
                
                // Get orders for this seller
                var orders = await _orderService.GetOrdersBySellerId(user.ClientId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving seller orders");
                return StatusCode(500, "An error occurred while retrieving orders");
            }
        }

        [Authorize]
        [HttpGet("seller/{id}")]
        public async Task<ActionResult<Order>> GetSellerOrderDetail(string id)
        {
            try
            {
                // Get current user information
                var user = await _userService.GetUserByEmail();
                
                // Check if user is a seller
                if (user.UserRole != "Seller")
                {
                    return Forbid("Only sellers can access seller orders");
                }
                
                // Get the full order
                var order = await _orderService.GetById(id);
                if (order == null)
                {
                    return NotFound();
                }
                
                // Check if this seller has any line items in this order
                bool hasSellerItems = order.LineItems.Any(li => li.SellerId == user.ClientId);
                if (!hasSellerItems)
                {
                    return NotFound("No items from this seller in the specified order");
                }
                
                // Filter line items to only show this seller's items
                order.LineItems = order.LineItems
                    .Where(li => li.SellerId == user.ClientId)
                    .ToList();
                
                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving seller order details");
                return StatusCode(500, "An error occurred while retrieving order details");
            }
        }

        [Authorize]
        [HttpGet("customer")]
        public async Task<ActionResult<List<OrderSearchResponse>>> GetCustomerOrders()
        {
            try
            {
                // Get current user information
                var user = await _userService.GetUserByEmail();
                
                // Filter orders by client ID to get this customer's orders
                var filter = Builders<Order>.Filter.Eq(o => o.ClientId, user.ClientId);
                var orders = await _context.Orders
                    .Find(filter)
                    .SortByDescending(x => x.CreatedAt)
                    .ToListAsync();
                
                var orderResponses = _mapper.Map<List<OrderSearchResponse>>(orders);
                return Ok(orderResponses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer orders");
                return StatusCode(500, "An error occurred while retrieving orders");
            }
        }

        [Authorize]
        [HttpGet("customer/{id}/grouped")]
        public async Task<ActionResult<List<Order>>> GetCustomerOrderGroupedBySeller(string id)
        {
            try
            {
                // Get current user information
                var user = await _userService.GetUserByEmail();
                
                // Get the order
                var order = await _orderService.GetById(id);
                if (order == null)
                {
                    return NotFound();
                }
                
                // Check if this user owns the order
                if (order.ClientId != user.ClientId)
                {
                    return Forbid("You don't have permission to view this order");
                }
                
                // Get the order grouped by seller
                var groupedOrders = await _orderService.GetOrderGroupedBySeller(id);
                return Ok(groupedOrders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving grouped order");
                return StatusCode(500, "An error occurred while retrieving the order");
            }
        }
    }
}


