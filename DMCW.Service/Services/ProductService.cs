using AutoMapper;
using DMCW.Repository.Data.DataService;
using DMCW.Repository.Data.Entities.product;
using DMCW.Service.Helper;
using DMCW.Service.Services.blob;
using DMCW.ServiceInterface.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;


namespace DMCW.Service.Services
{
    public class ProductService : IProductService
    {
        private readonly ILogger<ProductService> _logger;
        private readonly BlobService _blobService;
        private readonly MongoDBContext _context;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITagsService _tagsService;

        private string _clientId => Utility.GetUserIdFromClaims(_httpContextAccessor);

        public ProductService(ILogger<ProductService> logger, MongoDBContext context, BlobService blobService,
            IMapper mapper, IHttpContextAccessor httpContextAccessor, ITagsService tagsService)
        {
            _logger = logger;
            _blobService = blobService;
            _context = context;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _tagsService = tagsService;
        }

        public async Task<IEnumerable<ProductSearchResponse>> GetAllProductsAsync()
        {
            var res = await _context.Products.Find(FilterDefinition<Product>.Empty)
                                             .SortByDescending(x => x.CreatedAt)
                                             .Limit(1000)
                                             .ToListAsync();

            return _mapper.Map<IList<ProductSearchResponse>>(res);
        }

        public async Task<Product> GetProductByIdAsync(string id)
        {
            var filter = Builders<Product>.Filter.Eq(x => x.Id, id);
            return await _context.Products.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<string> CreateProductAsync(Product product)
        {
            var imgUrls = await _blobService.UploadMedia(product.Images);
            if (imgUrls.Count > 0)
            {
                product.ImgUrls.AddRange(imgUrls);
            }

            product.Images = null;
            product.CreatedAt = DateTime.Now;

            await _context.Products.InsertOneAsync(product);

            return product.Id;
        }

        public async Task<bool> UpdateProductAsync(Product product)
        {
            var imgUrls = await _blobService.UploadMedia(product.Images);
            if (imgUrls.Count > 0)
            {
                product.ImgUrls.AddRange(imgUrls);
            }
            product.Images = null;
            product.UpdatedAt = DateTime.Now;
            var filter = Builders<Product>.Filter.Eq(x => x.Id, product.Id);
            var result = await _context.Products.ReplaceOneAsync(filter, product);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteProductAsync(string id)
        {
            var filter = Builders<Product>.Filter.Eq(x => x.Id, id);
            var result = await _context.Products.SoftDeleteOneAsync(filter);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

 
        public async Task<bool> Update(string id, string title, string description)
        {
            var filter = Builders<Product>.Filter.Eq(x => x.Id, id);
            var update = Builders<Product>.Update.Set(x => x.Title, title)
                                                    .Set(x => x.Description, description);
            var result = await _context.Products.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

   

        public async Task<List<ProductSearchResponse>> SearchProductsAsync(string? query)
        {
            var result = new List<Product>();

            if (string.IsNullOrEmpty(query))
            {
                result = await _context.Products
                                       .Find(FilterDefinition<Product>.Empty)
                                       .SortByDescending(x => x.CreatedAt)
                                       .Limit(20)
                                       .ToListAsync();
            }
            else
            {

                var pipeline = QueryBuilder.BuildSearchFilter(query, "product_search_index", _clientId, new List<string> { "Title" });
                result = await _context.Products.Aggregate()
                         .AppendStage<Product>(pipeline[0])
                         .AppendStage<Product>(pipeline[1])
                         .SortByDescending(x => x.CreatedAt)
                         .Limit(20)
                         .ToListAsync();

            }

            _logger.LogInformation("Search Results Count: {ResultCount}", result.Count);

            return _mapper.Map<List<ProductSearchResponse>>(result);
        }
        public async Task AddTagToProduct(string productId, List<string> tagNames)
        {
            var tags = new List<Tag>();

            foreach (var tagName in tagNames)
            {
                var tag = await _tagsService.AddOrUpdateTagAsync(tagName, "product");

                tags.Add(tag);
            }
            var tagNamesToAdd = tags.Select(t => t.Name).ToList();

            var productFilter = Builders<Product>.Filter.Eq(x => x.Id, productId);

            var update = Builders<Product>.Update.Set(x => x.Tags, tagNamesToAdd);

            await _context.Products.UpdateOneAsync(productFilter, update);
        }

        public async Task<bool> UpdateProductMediaAsync(MediaServiceDto mediaServiceDto)
        {
            var filter = Builders<Product>.Filter.Eq(x => x.Id, mediaServiceDto.productId);
            var product = await _context.Products.Find(filter).FirstOrDefaultAsync();

            if (product == null)
            {
                throw new Exception("Product not found");
            }

            List<string> newImageUrls = new List<string>();
            foreach (var base64String in mediaServiceDto.newMediaBase64)
            {
                var blobClient = await _blobService.UploadToBlobAsync(base64String);
                newImageUrls.Add(blobClient.AbsoluteUri);
            }

            if (product.ImgUrls == null)
            {
                product.ImgUrls = new List<string>();
            }
            product.ImgUrls.AddRange(newImageUrls);

            var urlsToDelete = mediaServiceDto.mediaUpdates
                .Where(mu => mu.IsDeleted)
                .Select(mu => mu.Url)
                .ToHashSet();

            if (urlsToDelete.Any())
            {
                product.ImgUrls = product.ImgUrls
                    .Where(url => !urlsToDelete.Contains(url))
                    .ToList();
            }

            var result = await _context.Products.ReplaceOneAsync(filter, product);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }
        public async Task<SearchResultServiceDto<Product>> AdvanceSearch(SearchFilterServiceDto searchFilterServiceDto)
        {
            var queryText = searchFilterServiceDto.SearchFilterRequest?.QueryText;
            var filtersExist = searchFilterServiceDto.SearchFilterRequest.Filters.Count > 0;
            var pageSize = searchFilterServiceDto.End - searchFilterServiceDto.Start;

            var (products, totalCount) = await FetchProducts(searchFilterServiceDto, queryText, filtersExist, pageSize);

            if (filtersExist)
            {
                products = SearchFilterHelper.ApplyInMemoryFiltering(products, searchFilterServiceDto.SearchFilterRequest.Filters);
                totalCount = products.Count;
            }

            return products.Any()
                ? new SearchResultServiceDto<Product> { Records = products, TotalRecords = totalCount }
                : null;
        }
        private async Task<(List<Product>, int)> FetchProducts(SearchFilterServiceDto searchFilterServiceDto, string queryText, bool filtersExist, int pageSize)
        {
            if (!string.IsNullOrEmpty(queryText))
            {
                return await FetchProductsWithQuery(queryText);
            }
            else if (filtersExist)
            {
                return await FetchProductsWithFilters(pageSize);
            }
            else
            {
                return await FetchAllProducts(pageSize);
            }
        }

        private async Task<(List<Product>, int)> FetchProductsWithQuery(string queryText)
        {
            var pipeline = QueryBuilder.BuildSearchFilter(queryText, "product_search_index", _clientId, new List<string> { "Title", "Variants.Barcode" });
            var findQuery = await _context.Products.AggregateAsync();

            var products = await findQuery
                .AppendStage<Product>(pipeline[0])
                .AppendStage<Product>(pipeline[1])
                .SortByDescending(x => x.CreatedAt)
                .Limit(20)
                .ToListAsync();

            return (products, products.Count);
        }

        private async Task<(List<Product>, int)> FetchProductsWithFilters(int pageSize)
        {
            var findQuery = await _context.Products.FindAsync(FilterDefinition<Product>.Empty);
            var products = await findQuery
                .SortByDescending(x => x.CreatedAt)
                .Limit(pageSize)
                .ToListAsync();

            return (products, products.Count);
        }

        private async Task<(List<Product>, int)> FetchAllProducts(int pageSize)
        {
            var findQuery = await _context.Products.FindAsync(FilterDefinition<Product>.Empty);
            var products = await findQuery
                .SortByDescending(x => x.CreatedAt)
                .Limit(pageSize)
                .ToListAsync();

            return (products, products.Count);
        }
    }
}
