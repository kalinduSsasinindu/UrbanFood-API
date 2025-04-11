using DMCW.Repository.Data.DataService;
using DMCW.Repository.Data.Entities.Order;
using DMCW.Repository.Data.Entities.product;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace DMCW.Service.Services
{
    /// <summary>
    /// Example service that uses both MongoDB and Oracle databases
    /// </summary>
    public class DualDatabaseService
    {
        private readonly MongoDBContext _mongoContext;
        private readonly OracleDBContext _oracleContext;
        private readonly ILogger<DualDatabaseService> _logger;

        public DualDatabaseService(
            MongoDBContext mongoContext,
            OracleDBContext oracleContext,
            ILogger<DualDatabaseService> logger)
        {
            _mongoContext = mongoContext;
            _oracleContext = oracleContext;
            _logger = logger;
        }

        /// <summary>
        /// Example method that reads from MongoDB and writes to Oracle
        /// </summary>
        public async Task SyncProductsToOracle()
        {
            try
            {
                // Get products from MongoDB
                var products = await _mongoContext.Products
                    .Find(FilterDefinition<Product>.Empty)
                    .ToListAsync();

                _logger.LogInformation($"Syncing {products.Count} products to Oracle");

                // Insert products into Oracle
                foreach (var product in products)
                {
                    try
                    {
                        // Check if product exists in Oracle
                        var exists = await _oracleContext.Products.FindOneAsync(
                            $" WHERE ID = :id",
                            new Dictionary<string, object> { { "id", product.Id } }
                        );

                        if (exists == null)
                        {
                            // Insert new product
                            await _oracleContext.Products.InsertOneAsync(product);
                            _logger.LogInformation($"Product {product.Id} inserted into Oracle");
                        }
                        else
                        {
                            // Update existing product
                            await _oracleContext.Products.ReplaceOneAsync(product.Id, product);
                            _logger.LogInformation($"Product {product.Id} updated in Oracle");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error syncing product {product.Id} to Oracle");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing products to Oracle");
                throw;
            }
        }

        /// <summary>
        /// Example method that loads customer data from Oracle to MongoDB
        /// </summary>
       

        /// <summary>
        /// Example of a transaction that spans both databases (not atomic across both)
        /// </summary>
        public async Task CreateOrderInBothDatabases(Order order)
        {
            // First create in MongoDB
            try
            {
                await _mongoContext.Orders.InsertOneAsync(order);
                _logger.LogInformation($"Order {order.Id} created in MongoDB");

                // Then create in Oracle
                try
                {
                    await _oracleContext.Orders.InsertOneAsync(order);
                    _logger.LogInformation($"Order {order.Id} created in Oracle");
                }
                catch (Exception ex)
                {
                    // Oracle failed, rollback MongoDB
                    _logger.LogError(ex, $"Oracle insert failed for order {order.Id}, rolling back MongoDB");
                    
                    var filter = Builders<Order>.Filter.Eq(o => o.Id, order.Id);
                    await _mongoContext.Orders.DeleteOneAsync(filter);
                    
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to create order {order.Id} in both databases");
                throw;
            }
        }
    }
} 