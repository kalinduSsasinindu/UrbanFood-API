using DMCW.Repository.Data.Entities.product;
using DMCW.Repository.Data.Entities.Search;
using DMCW.ServiceInterface.Dtos;
using DMCW.ServiceInterface.Dtos.product.DMCW.API.Dtos.Product.DMCW.API.Dtos;
using DMCW.Shared.Utility.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMCW.ServiceInterface.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductSearchResponse>> GetAllProductsAsync();
        Task<Product> GetProductByIdAsync(string id);
        Task<string> CreateProductAsync(Product product);
        Task<bool> UpdateProductAsync(Product product);
        Task<bool> DeleteProductAsync(string id);
        Task<bool> Update(string id, List<ProductVariant> Variants);
        Task<bool> Update(string id, List<VariantOption> Options);
        Task<bool> Update(string id, string title, string description);
        Task<List<ProductSearchResponse>> SearchProductsAsync(string? query);
        Task<bool> UpdateProductMediaAsync(MediaServiceDto mediaServiceDto);
        Task AddTagToProduct(string productId, List<string> tagNames);
        Task<Product> GetProductDetailsByIdAsync(string id);
        Task<List<Product>> GetProductsByProductTypeAsync(ProductType? productType);
        Task<ProductReview> AddProductReviewAsync(string productId, string userId, CreateProductReviewDto reviewDto);
        Task<List<ProductReview>> GetProductReviewsAsync(string productId);
    }
}
