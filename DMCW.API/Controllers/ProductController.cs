using AutoMapper;
using DMCW.API.Dtos;
using DMCW.Repository.Data.Entities.product;
using DMCW.Repository.Data.Entities.Search;
using DMCW.ServiceInterface.Dtos;
using DMCW.ServiceInterface.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMCW.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ILogger<ProductController> _logger;
        private readonly IProductService _productService;
        private readonly IMapper _mapper;

        public ProductController(ILogger<ProductController> logger, IProductService productService, IMapper mapper)
        {
            _logger = logger;
            _productService = productService;
            _mapper = mapper;
        }

        [Authorize]
        [HttpGet]
        public async Task<IEnumerable<ProductSearchResponse>> Get()
        {
            return await _productService.GetAllProductsAsync();
        }

        [Authorize]
        [HttpGet("{id}")]
        [ActionName(nameof(GetById))]
        public async Task<ActionResult<Product>> GetById(string id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            return product is not null ? Ok(product) : NotFound();
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> Create(Product product)
        {
            var id = await _productService.CreateProductAsync(product);
            return CreatedAtAction(nameof(GetById), new { id = id }, product);
        }

        [Authorize]
        [HttpPut]
        public async Task<ActionResult> Update(Product product)
        {
            var success = await _productService.UpdateProductAsync(product);
            return success ? Ok() : NotFound();
        }



        [Authorize]
        [HttpPatch("{id}/titledescription")]
        public async Task<ActionResult> Update(string id, string title, string description)
        {
            var success = await _productService.Update(id, title, description);
            return success ? Ok() : NotFound();
        }


        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            var success = await _productService.DeleteProductAsync(id);
            return success ? Ok() : NotFound();
        }


        [Authorize]
        [HttpGet("product")]
        public async Task<ActionResult<List<Product>>> SearchProduct([FromQuery] string? query)
        {
            var result = await _productService.SearchProductsAsync(query);
            return Ok(result);
        }

        [Authorize]
        [HttpPut("media")]
        public async Task<ActionResult> Update(MediaWebDto mediaWebDto)
        {
            var media = _mapper.Map<MediaServiceDto>(mediaWebDto);
            var success = await _productService.UpdateProductMediaAsync(media);
            return success ? Ok() : NotFound();
        }

        [HttpPost("{productId}/add-tag")]
        public async Task<IActionResult> AddTagToOrder(string productId, List<string> tagNames)
        {
            await _productService.AddTagToProduct(productId, tagNames);
            return Ok();
        }
    }
}
