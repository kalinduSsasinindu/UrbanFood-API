using AutoMapper;
using CloudinaryDotNet.Actions;
using DMCW.Repository.Data.DataService;
using DMCW.Repository.Data.Entities.product;
using DMCW.Service.Helper;
using DMCW.ServiceInterface.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DMCW.Repository.Helper;
using MongoDB.Driver;
using DMCW.Repository.Data.Entities.Order;
using Tag = DMCW.Repository.Data.Entities.Tags.Tag;
using DMCW.Service.Services.blob;
using DMCW.Repository.Data.Entities.Search;
using DMCW.Repository.Data.Entities.User;
namespace DMCW.Service.Services
{
    public class OrderService : IOrderService
    {
        private readonly MongoDBContext _context;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly CloudinaryService _blobService;
        private readonly ILogger<OrderService> _logger;
        private readonly ITagsService _tagsService;
        private string _clientId => Utility.GetUserIdFromClaims(_httpContextAccessor);
        private readonly IConfiguration _configuration;
        private string _shopifyApiBaseUrl;
        private string _shopifyAccessToken;
        private readonly string _requestFilter;


        public OrderService(MongoDBContext mongoDBContext, IMapper mapper, IHttpContextAccessor httpContextAccessor,
            HttpClient httpClient, ILogger<OrderService> logger, IConfiguration configuration, CloudinaryService blobService, ITagsService tagsService)
        {
            _context = mongoDBContext;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
            _requestFilter = _configuration["Shopify:RequestFilter"];
            _tagsService = tagsService;
            _blobService = blobService;
        }

        public async Task<IEnumerable<OrderSearchResponse>> GetOrders()
        {
            var orders = await _context.Orders.Find(FilterDefinition<Order>.Empty)
            .SortByDescending(x => x.CreatedAt)
                                                .Limit(50)
                                                .ToListAsync();
            var filteredRes = _mapper.Map<IList<OrderSearchResponse>>(orders);
            return filteredRes;
        }



        public async Task<IEnumerable<OrderSearchResponse>> GetOrdersByStatus(string fulfilmentStatus)
        {
            var filter = Builders<Order>.Filter.Eq(x => x.FulfillmentStatus, fulfilmentStatus);
            var orders = await _context.Orders.Find(filter)
                                                .SortByDescending(x => x.CreatedAt)
                                                .Limit(50)
                                                .ToListAsync();
            var filteredRes = _mapper.Map<IList<OrderSearchResponse>>(orders);
            return filteredRes;
        }





        private async Task<(List<Order>, int)> FetchOrdersWithoutQuery(int start, int pageSize)
        {
            var totalCount = await _context.Orders.CountDocumentsAsync(FilterDefinition<Order>.Empty);
            var orders = await _context.Orders.Find(FilterDefinition<Order>.Empty)
                .SortByDescending(x => x.CreatedAt)
                .Skip(start)
                .Limit(pageSize)
                .ToListAsync();

            return (orders, (int)totalCount);
        }








        public async Task<List<Order>> GetByIds(List<string> ids)
        {
            var filter = Builders<Order>.Filter.In(x => x.Id, ids);
            var orders = await _context.Orders.Find(filter).ToListAsync();
            return orders;
        }

        public async Task Create(Order order)
        {
            order.Name = await GenerateOrderName(order);
            order.CreatedAt = DateTime.Now;
            order.Tags = new List<string>();

            var productIds = order.LineItems.Select(li => li.ProductId).Distinct().ToList();

            // Use the new method that bypasses clientId filtering
            var products = await GetProductsByIdsWithoutUserFilter(productIds);

            foreach (var lineItem in order.LineItems)
            {
                var product = products.FirstOrDefault(p => p.Id == lineItem.ProductId);
                var productVariant = GetProductVariant(product, lineItem.VariantId);

                // Set the seller information for each line item
                lineItem.SellerId = product.ClientId;
                
                // Get seller name from user service
                try {
                    var seller = await _context.Users.Find(Builders<User>.Filter.Eq(u => u.ClientId, product.ClientId)).FirstOrDefaultAsync();
                    lineItem.SellerName = seller?.Name ?? "Unknown Seller";
                }
                catch {
                    lineItem.SellerName = "Unknown Seller";
                }

                ValidateStockAvailability(productVariant, lineItem.Quantity);

                UpdateStockQuantities(productVariant, lineItem.Quantity);

                await UpdateProductStock(product);
            }

            await _context.Orders.InsertOneAsync(order);
        }
        private async Task UpdateProductStock(Product product)
        {
            // Get the base collection directly to bypass clientId filtering
            var baseCollection = _context.GetBaseCollection<Product>("Product");
            
            var updateFilter = Builders<Product>.Filter.Eq(p => p.Id, product.Id);
            var updateDefinition = Builders<Product>.Update.Set(p => p.Variants, product.Variants);

            var result = await baseCollection.UpdateOneAsync(updateFilter, updateDefinition);

            if (result.ModifiedCount == 0)
            {
                throw new Exception($"Failed to update stock for product {product.Title}. Stock might have been modified by another process.");
            }
        }
        private async Task<string> GenerateOrderName(Order order)
        {
            var orderNumber = await Utility.GetNextSequenceValue("orderid", _clientId, _context);
            return $"#{orderNumber + 1000}";
        }

        private async Task<Product> GetProductById(string productId)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.Id, productId);
            var product = await _context.Products.Find(filter).FirstOrDefaultAsync();

            if (product == null)
            {
                throw new Exception($"Product with ID {productId} not found.");
            }

            return product;
        }

        private ProductVariant GetProductVariant(Product product, int variantId)
        {
            var productVariant = product.Variants.FirstOrDefault(v => v.VariantId == variantId);

            if (productVariant == null)
            {
                throw new Exception($"Product variant with ID {variantId} not found in product {product.Title}.");
            }

            return productVariant;
        }

        private void ValidateStockAvailability(ProductVariant productVariant, int requestedQuantity)
        {
            var availableQuantity = productVariant.AvailableQuantity.GetValueOrDefault();

            if (requestedQuantity > availableQuantity)
            {
                throw new Exception($"Not enough available stock for variant {productVariant.SKU}. Available: {availableQuantity}, Requested: {requestedQuantity}.");
            }
        }

        private void UpdateStockQuantities(ProductVariant productVariant, int quantity)
        {
            productVariant.AvailableQuantity -= quantity;
            productVariant.CommittedQuantity = productVariant.CommittedQuantity.GetValueOrDefault() + quantity;
        }

        private void UpdateDeletedStockQuantities(ProductVariant productVariant, int quantity)
        {
            productVariant.AvailableQuantity += quantity;
            productVariant.CommittedQuantity = productVariant.CommittedQuantity.GetValueOrDefault() - quantity;
        }

        private async Task<List<Product>> GetProductsByIds(List<string> productIds)
        {
            var filter = Builders<Product>.Filter.In(p => p.Id, productIds);
            var products = await _context.Products.Find(filter).ToListAsync();

            if (products.Count != productIds.Count)
            {
                var missingIds = productIds.Except(products.Select(p => p.Id));
                throw new Exception($"Products with IDs {string.Join(", ", missingIds)} not found.");
            }

            return products;
        }

        private async Task<List<Product>> GetProductsByIdsWithoutUserFilter(List<string> productIds)
        {
            // Get the base collection directly, bypassing the filtered collection
            var baseCollection = _context.GetBaseCollection<Product>("Product");

            // Create filter only for the product IDs without clientId filtering
            var filter = Builders<Product>.Filter.And(
                Builders<Product>.Filter.In(p => p.Id, productIds),
                Builders<Product>.Filter.Eq("IsDeleted", false)
            );

            // Query the base collection directly
            var products = await baseCollection.Find(filter).ToListAsync();

            if (products.Count != productIds.Count)
            {
                var missingIds = productIds.Except(products.Select(p => p.Id));
                throw new Exception($"Products with IDs {string.Join(", ", missingIds)} not found.");
            }

            return products;
        }

        private async Task<List<Product>> GetProductsByIdsForCustomer(List<string> productIds)
        {
            var filter = Builders<Product>.Filter.In(p => p.Id, productIds);
            var products = await _context.Products.Find(filter).ToListAsync();

            if (products.Count != productIds.Count)
            {
                var missingIds = productIds.Except(products.Select(p => p.Id));
                throw new Exception($"Products with IDs {string.Join(", ", missingIds)} not found.");
            }

            return products;
        }
        public async Task Update(string id, List<LineItem> lineItems)
        {
            var order = await GetOrderById(id);

            var fulfilledLineItems = lineItems.Where(x => x.FulfillmentStatus == "tofulfill");
            var productIds = fulfilledLineItems.Select(li => li.ProductId).Distinct().ToList();
            var products = await GetProductsByIds(productIds);

            foreach (var lineItem in fulfilledLineItems)
            {
                var product = products.FirstOrDefault(p => p.Id == lineItem.ProductId);
                if (product == null)
                {
                    continue;
                }

                var productVariant = GetProductVariant(product, lineItem.VariantId);
                if (productVariant == null)
                {
                    continue;
                }

                AdjustCommittedQuantity(productVariant, lineItem.Quantity);

                await UpdateProductStock(product);
            }

            var lineItemsToFulfill = lineItems.Where(x => x.FulfillmentStatus == "tofulfill").ToList();

            foreach (var lineItem in lineItemsToFulfill)
            {
                lineItem.FulfillmentStatus = "fulfilled";
            }

            var lineItemUpdates = Builders<Order>.Update.Set(x => x.LineItems, lineItems);
            await _context.Orders.UpdateOneAsync(Builders<Order>.Filter.Eq(x => x.Id, id), lineItemUpdates);


            await UpdateOrderFulfilledStatus(id);
            await UpdateStock(order, lineItems);
        }

        private async Task UpdateStock(Order order, List<LineItem> lineItems)
        {
            await UpdateInventoryForDeletedItems(order, lineItems);
            await UpdateInventoryForAddedItems(order, lineItems);
            await UpdateInventoryForUpdatedQuantities(order, lineItems);
        }

        private async Task UpdateInventoryForDeletedItems(Order order, List<LineItem> lineItems)
        {
            List<LineItem> excludedLineItems = order.LineItems
                                                .Where(li => !lineItems.Any(oli => oli.ProductId == li.ProductId && oli.VariantId == li.VariantId))
                                                .ToList();
            foreach (LineItem lineItem1 in excludedLineItems)
            {
                var productIds = order.LineItems.Select(li => li.ProductId).Distinct().ToList();

                if (productIds == null || !productIds.Any())
                {
                    throw new Exception("No product IDs were found in the provided line items.");
                }

                var products = await GetProductsByIds(productIds);

                var product = products.FirstOrDefault(p => p.Id == lineItem1.ProductId);
                var productVariant = GetProductVariant(product, lineItem1.VariantId);

                UpdateDeletedStockQuantities(productVariant, lineItem1.Quantity);
                await UpdateProductStock(product);
            }

            order.LineItems.RemoveAll(li => excludedLineItems.Contains(li));
        }

        private async Task UpdateInventoryForAddedItems(Order order, List<LineItem> lineItems)
        {
            List<LineItem> excludedLineItems = lineItems
                                   .Where(li => !order.LineItems.Any(oli => oli.ProductId == li.ProductId && oli.VariantId == li.VariantId))
                                   .ToList();
            foreach (LineItem lineItem2 in excludedLineItems)
            {
                var productIds = lineItems.Select(li => li.ProductId).Distinct().ToList();

                if (productIds == null || !productIds.Any())
                {
                    throw new Exception("No product IDs were found in the provided line items.");
                }

                var products = await GetProductsByIds(productIds);

                var product = products.FirstOrDefault(p => p.Id == lineItem2.ProductId);
                var productVariant = GetProductVariant(product, lineItem2.VariantId);

                ValidateStockAvailability(productVariant, lineItem2.Quantity);

                UpdateStockQuantities(productVariant, lineItem2.Quantity);

                await UpdateProductStock(product);
            }

            order.LineItems.AddRange(excludedLineItems);
        }

        private async Task UpdateInventoryForUpdatedQuantities(Order order, List<LineItem> lineItems)
        {
            foreach (LineItem lineItem in lineItems)
            {
                var productIds = lineItems.Select(li => li.ProductId).Distinct().ToList();

                if (productIds == null || !productIds.Any())
                {
                    throw new Exception("No product IDs were found in the provided line items.");
                }

                var products = await GetProductsByIds(productIds);

                var product = products.FirstOrDefault(p => p.Id == lineItem.ProductId);
                var productVariant = GetProductVariant(product, lineItem.VariantId);

                var orderLineItem = order.LineItems
                                    .Where(li => li.ProductId == lineItem.ProductId && li.VariantId == lineItem.VariantId).FirstOrDefault();

                if (orderLineItem == null)
                {
                    throw new Exception($"Line item with Product ID {lineItem.ProductId} and Variant ID {lineItem.VariantId} not found in Order ID {order.Id}.");
                }

                if (orderLineItem.Quantity < lineItem.Quantity)
                {
                    ValidateStockAvailability(productVariant, lineItem.Quantity - orderLineItem.Quantity);

                    UpdateStockQuantities(productVariant, lineItem.Quantity - orderLineItem.Quantity);
                    await UpdateProductStock(product);
                }

                if (orderLineItem.Quantity > lineItem.Quantity)
                {
                    UpdateDeletedStockQuantities(productVariant, orderLineItem.Quantity - lineItem.Quantity);
                    await UpdateProductStock(product);
                }
            }
        }

        private async Task<Order> GetOrderById(string orderId)
        {
            var orderFilter = Builders<Order>.Filter.Eq(x => x.Id, orderId);
            var order = await _context.Orders.Find(orderFilter).FirstOrDefaultAsync();

            if (order == null)
            {
                throw new Exception($"Order with ID {orderId} not found.");
            }

            return order;
        }

        private void AdjustCommittedQuantity(ProductVariant productVariant, int quantity)
        {
            var newCommittedQuantity = productVariant.CommittedQuantity.GetValueOrDefault() - quantity;

            if (newCommittedQuantity < 0)
            {
                throw new Exception($"Committed quantity for variant {productVariant.SKU} is insufficient. Available committed: {productVariant.CommittedQuantity}, Requested: {quantity}.");
            }

            productVariant.CommittedQuantity = newCommittedQuantity;
        }

        public async Task Update(string id, ShippingAddress shippingAddress)
        {
            var filter = Builders<Order>.Filter.Eq(x => x.Id, id);
            var update = Builders<Order>.Update.Set(x => x.ShippingAddress, shippingAddress);
            var result = await _context.Orders.UpdateOneAsync(filter, update);

        }

        public async Task Update(string id, PaymentInfo paymentInfo)
        {
            var filter = Builders<Order>.Filter.Eq(x => x.Id, id);
            var update = Builders<Order>.Update.Set(x => x.PaymentInfo, paymentInfo);
            var result = await _context.Orders.UpdateOneAsync(filter, update);
            await UpdateOrderPaymentStatus(id);
        }

        public async Task UpdateFulfillStatus(string id, string fulfillStatus)
        {
            var filter = Builders<Order>.Filter.Eq(x => x.Id, id);
            var update = Builders<Order>.Update.Set(x => x.FulfillmentStatus, fulfillStatus);
            var result = await _context.Orders.UpdateOneAsync(filter, update);
            await AddTimelineAsync(id, new TimeLineDetails
            {
                CreatedAt = DateTime.Now,
                Comment = $"fulfillment status updated to {fulfillStatus}"
            });
        }

        public async Task UpdatePaymentStatus(string id, string paymentStatus, decimal amount = 0, string paymentMethod = null)
        {
            var filter = Builders<Order>.Filter.Eq(x => x.Id, id);
            var update = Builders<Order>.Update.Set(x => x.FinancialStatus, paymentStatus);
            var result = await _context.Orders.UpdateOneAsync(filter, update);
            await AddTimelineAsync(id, new TimeLineDetails
            {
                CreatedAt = DateTime.Now,
                Comment = $"payment status updated {paymentStatus} amount paid {amount} LKR via {paymentMethod}"
            });
        }

        public async Task<Order> GetById(string id)
        {
            var filter = Builders<Order>.Filter.Eq(x => x.Id, id);
            var order = await _context.Orders.Find(filter).FirstOrDefaultAsync();
            return order;
        }

        public async Task<List<OrderSearchResponse>> SearchOrdersAsync(string? query)
        {
            var result = new List<Order>();

            if (string.IsNullOrEmpty(query))
            {
                result = await _context.Orders
                                       .Find(FilterDefinition<Order>.Empty)
                                       .SortByDescending(x => x.CreatedAt)
                                       .Limit(20)
                                       .ToListAsync();
            }
            else
            {
                var pipeline = QueryBuilder.BuildSearchFilter(query, "order_index", _clientId, new List<string> { "Name", "ShippingAddress.FirstName", "ShippingAddress.LastName" });

                result = await _context.Orders.Aggregate()
                         .AppendStage<Order>(pipeline[0])
                         .AppendStage<Order>(pipeline[1])
                         .SortByDescending(x => x.CreatedAt)
                         .Limit(20)
                         .ToListAsync();
            }
            return _mapper.Map<List<OrderSearchResponse>>(result);
        }

        public async Task AddTimelineAsync(string orderId, TimeLineDetails details)
        {
            var imgUrls = await _blobService.UploadMedia(details.Images);
            details.ImgUrls = details.ImgUrls ?? new List<string>();
            if (imgUrls.Count > 0)
            {
                details.ImgUrls.AddRange(imgUrls);
            }

            details.Images = null;
            details.CreatedAt = DateTime.Now;

            var filter = Builders<Order>.Filter.Eq(x => x.Id, orderId);
            var update = Builders<Order>.Update.Push(x => x.TimeLineDetails, details);
            var result = await _context.Orders.UpdateOneAsync(filter, update);
            await Task.CompletedTask;
        }

        private async Task UpdateOrderFulfilledStatus(string id)
        {
            var order = await GetById(id);
            if (order == null)
            {
                return;
            }

            int totalLineItemCount = order.LineItems.Count;
            int fulfilledCount = order.LineItems.Count(item => item.FulfillmentStatus == "fulfilled");

            string orderFulfillStatus = fulfilledCount switch
            {
                var count when count == totalLineItemCount => "fulfilled",
                var count when count > 0 => "partially_fulfilled",
                _ => null
            };

            await UpdateFulfillStatus(id, orderFulfillStatus);
        }

        private async Task UpdateOrderPaymentStatus(string id)
        {
            var order = await GetById(id);
            if (order == null)
            {
                return;
            }

            decimal totalPaidAmount = order.PaymentInfo.TotalPaidAmount;
            decimal totalPrice = order.TotalPrice;

            string orderFulfillStatus = totalPaidAmount switch
            {
                var count when count == totalPrice => "paid",
                var count when count == 0 => "pending",
                var count when count < totalPrice => "partially_paid",
                _ => "pending"
            };
            var latestPayment = order.PaymentInfo.Payments != null ? order.PaymentInfo.Payments.LastOrDefault() : null;
            if (latestPayment == null)
            {
                return;
            }
            string paymentMethod = EnumHelper.GetEnumDescription<PaymentOptions>(Convert.ToInt16(latestPayment.PaymentMethod));
            await UpdatePaymentStatus(id, orderFulfillStatus, latestPayment.Amount, string.IsNullOrEmpty(paymentMethod) ? "Unknown" : paymentMethod);

        }


        public async Task<bool> DeleteProductAsync(string id)
        {
            var filter = Builders<Order>.Filter.Eq(x => x.Id, id);
            var result = await _context.Orders.SoftDeleteOneAsync(filter);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> CancelOrderAsync(string id)
        {
            var filter = Builders<Order>.Filter.Eq(x => x.Id, id);
            var update = Builders<Order>.Update.Set(x => x.IsCancelled, true);
            var result = await _context.Orders.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;

        }

        public async Task<bool> Update(string id, decimal subtotalPrice, decimal totalLineItemsPrice, decimal totalPrice, decimal totalShippingPrice, decimal totalDiscountPrice)
        {
            var filter = Builders<Order>.Filter.Eq(x => x.Id, id);

            var update = Builders<Order>.Update
                .Set(x => x.SubtotalPrice, subtotalPrice)
                .Set(x => x.TotalLineItemsPrice, totalLineItemsPrice)
                .Set(x => x.TotalPrice, totalPrice)
                .Set(x => x.TotalShippingPrice, totalShippingPrice)
                .Set(x => x.TotalDiscountPrice, totalDiscountPrice);

            var result = await _context.Orders.UpdateOneAsync(filter, update);

            await UpdateOrderPaymentStatus(id);

            return result.IsAcknowledged && result.ModifiedCount > 0;
        }


        private void SetHttpClientHeaders()
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-Shopify-Access-Token", _shopifyAccessToken);
        }

        private async Task<string> GetOrdersAsync(string url = null)
        {
            var requestUrl = url ?? $"{_shopifyApiBaseUrl}orders.json{_requestFilter}";

            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            HttpResponseMessage response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }





        private async Task ProcessOrderAsync(Order order)
        {
            try
            {
                var filter = Builders<Order>.Filter.And(
                                Builders<Order>.Filter.Eq(o => o.Name, order.Name),
                                Builders<Order>.Filter.Eq(o => o.ClientId, _clientId));
                var deleteResult = await _context.Orders.SoftDeleteOneAsync(filter);

                if (deleteResult.ModifiedCount > 0)
                {
                    _logger.LogInformation($"Soft Deleted existing order with Name: {order.Name}");
                }
                else
                {
                    _logger.LogInformation($"No existing order found with Name: {order.Name}");
                }
                await _context.Orders.InsertOneAsync(order);
                _logger.LogInformation($"Inserted new order with Name: {order.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing order with Name: {order.Name}. Exception: {ex.Message}");
            }
        }
        public async Task AddTagToOrder(string orderId, List<string> tagNames)
        {
            var tags = new List<Tag>();

            foreach (var tagName in tagNames)
            {
                var tag = await _tagsService.AddOrUpdateTagAsync(tagName, "order");

                tags.Add(tag);
            }
            var tagNamesToAdd = tags.Select(t => t.Name).ToList();

            var orderFilter = Builders<Order>.Filter.Eq(x => x.Id, orderId);

            var update = Builders<Order>.Update.Set(x => x.Tags, tagNamesToAdd);

            await _context.Orders.UpdateOneAsync(orderFilter, update);
        }

        public async Task<List<OrderSearchResponse>> GetOrdersBySellerId(string sellerId)
        {
            // Find all orders that have at least one line item with the specified sellerId
            var filter = Builders<Order>.Filter.ElemMatch(
                o => o.LineItems, 
                lineItem => lineItem.SellerId == sellerId
            );
            
            var orders = await _context.Orders
                .Find(filter)
                .SortByDescending(x => x.CreatedAt)
                .ToListAsync();
            
            var orderResponses = _mapper.Map<List<OrderSearchResponse>>(orders);
            
            return orderResponses;
        }

        public async Task<List<Order>> GetOrdersWithLineItemsBySellerId(string sellerId)
        {
            // Find all orders that have at least one line item with the specified sellerId
            var filter = Builders<Order>.Filter.ElemMatch(
                o => o.LineItems, 
                lineItem => lineItem.SellerId == sellerId
            );
            
            var orders = await _context.Orders
                .Find(filter)
                .SortByDescending(x => x.CreatedAt)
                .ToListAsync();
            
            // For each order, filter line items to only include those belonging to this seller
            foreach (var order in orders)
            {
                // Create a new list with only the line items for this seller
                var sellerLineItems = order.LineItems
                    .Where(li => li.SellerId == sellerId)
                    .ToList();
                
                // Replace the original line items with the filtered ones
                order.LineItems = sellerLineItems;
            }
            
            return orders;
        }

        public async Task<List<Order>> GetOrderGroupedBySeller(string orderId)
        {
            var order = await GetById(orderId);
            if (order == null)
            {
                return new List<Order>();
            }
            
            // Group line items by seller
            var lineItemsBySeller = order.LineItems
                .GroupBy(li => li.SellerId)
                .ToDictionary(g => g.Key, g => g.ToList());
            
            // Create a mini-order for each seller
            var sellerOrders = new List<Order>();
            foreach (var sellerId in lineItemsBySeller.Keys)
            {
                var sellerLineItems = lineItemsBySeller[sellerId];
                
                // Create a copy of the original order with just this seller's items
                var sellerOrder = new Order
                {
                    Id = order.Id,
                    Name = order.Name,
                    CreatedAt = order.CreatedAt,
                    ClientId = order.ClientId,
                    FulfillmentStatus = order.FulfillmentStatus,
                    FinancialStatus = order.FinancialStatus,
                    Note = order.Note,
                    Phone = order.Phone,
                    ShippingAddress = order.ShippingAddress,
                    Customer = order.Customer,
                    LineItems = sellerLineItems,
                    PaymentInfo = order.PaymentInfo,
                    TimeLineDetails = order.TimeLineDetails,
                    Tags = order.Tags,
                    // Calculate subtotals for just this seller's items
                    SubtotalPrice = sellerLineItems.Sum(li => li.Price * li.Quantity),
                    TotalLineItemsPrice = sellerLineItems.Sum(li => li.Price * li.Quantity),
                    // Keep the shipping and discounts the same
                    TotalShippingPrice = order.TotalShippingPrice,
                    TotalDiscountPrice = order.TotalDiscountPrice
                };
                
                // Calculate total price for this seller's portion
                sellerOrder.TotalPrice = sellerOrder.TotalLineItemsPrice + sellerOrder.TotalShippingPrice - sellerOrder.TotalDiscountPrice;
                
                // Add seller info to the order
                try {
                    var seller = await _context.Users.Find(Builders<User>.Filter.Eq(u => u.ClientId, sellerId)).FirstOrDefaultAsync();
                    sellerOrder.Note += $" (Seller: {seller?.Name ?? "Unknown"})";
                }
                catch {
                    // Ignore any errors getting seller info
                }
                
                sellerOrders.Add(sellerOrder);
            }
            
            return sellerOrders;
        }
    }
}