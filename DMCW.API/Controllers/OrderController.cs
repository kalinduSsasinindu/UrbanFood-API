using AutoMapper;
using CloudinaryDotNet.Actions;
using DMCW.API.Dtos.Order;
using DMCW.Repository.Data.Entities.Order;
using DMCW.Repository.Data.Entities.Search;
using DMCW.ServiceInterface.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        public OrderController(ILogger<OrderController> logger, IOrderService orderService, IMapper mapper, IUserService userService)
        {
            _logger = logger;
            _orderService = orderService;
            _mapper = mapper;
            _userService = userService;
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
    }
}


