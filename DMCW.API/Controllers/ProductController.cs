using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMCW.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : Controller
    {
        // A simple product class for demonstration
        public class Product
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
        }

        // Mock data of products
        private static List<Product> products = new List<Product>
        {
            new Product { Id = 1, Name = "Product 1", Price = 19.99m },
            new Product { Id = 2, Name = "Product 2", Price = 29.99m },
            new Product { Id = 3, Name = "Product 3", Price = 39.99m },
            new Product { Id = 4, Name = "Product 4", Price = 49.99m }
        };

        // This action requires authorization (user must be logged in)
       
        [HttpGet("products")]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                // Simulating some async work (e.g., database call)
                await Task.Delay(100);

                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving products.");
            }
        }

        // A public endpoint for testing (no authorization required)
        [HttpGet("public-products")]
        public IActionResult GetPublicProducts()
        {
            try
            {
                // Return a subset of products without authentication
                var publicProducts = products.GetRange(0, 2); // Only the first two products
                return Ok(publicProducts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving products.");
            }
        }
        [HttpGet("product/{productId}")]
        public IActionResult GetProductById(int productId)
        {
            try
            {
                // Find the product by its Id
                var product = products.FirstOrDefault(p => p.Id == productId);

                if (product == null)
                {
                    return NotFound($"Product with ID {productId} not found.");
                }

                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving the product.");
            }
        }
    }
}
