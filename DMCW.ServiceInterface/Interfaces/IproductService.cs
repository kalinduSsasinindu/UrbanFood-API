using DMCW.Repository.Data.Entities.product;
using DMCW.Repository.Data.Entities.Search;
using DMCW.ServiceInterface.Dtos;
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
       
        Task<bool> Update(string id, string title, string description);
        Task<List<ProductSearchResponse>> SearchProductsAsync(string? query);
        Task<bool> UpdateProductMediaAsync(MediaServiceDto mediaServiceDto);
        Task AddTagToProduct(string productId, List<string> tagNames);
    }
}
