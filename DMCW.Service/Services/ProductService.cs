using AutoMapper;
using DMCW.Repository.Data.DataService;
using DMCW.Repository.Data.Entities.product;
using DMCW.Repository.Data.Entities.Search;
using DMCW.Repository.Data.Entities.User;
using DMCW.Repository.Helper;
using DMCW.Service.Helper;
using DMCW.Service.Services.blob;
using DMCW.ServiceInterface.Dtos;
using DMCW.ServiceInterface.Interfaces;
using DMCW.Shared.Utility.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Tag = DMCW.Repository.Data.Entities.Tags.Tag;



namespace DMCW.Service.Services
{
    public class ProductService : IProductService
    {
        private readonly ILogger<ProductService> _logger;
        private readonly CloudinaryService _blobService;
        private readonly MongoDBContext _context;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITagsService _tagsService;
        private readonly IUserService _userService;

        private string _clientId => Utility.GetUserIdFromClaims(_httpContextAccessor);

        public ProductService(ILogger<ProductService> logger, MongoDBContext context, CloudinaryService blobService,
            IMapper mapper, IHttpContextAccessor httpContextAccessor, ITagsService tagsService, IUserService userService)
        {
            _logger = logger;
            _blobService = blobService;
            _context = context;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _tagsService = tagsService;
            _userService =userService;
        }

        public async Task<IEnumerable<ProductSearchResponse>> GetAllProductsAsync()
        {
            var res = await _context.Products.Find(FilterDefinition<Product>.Empty)
                                       .SortByDescending(x => x.CreatedAt)
                                       .Limit(1000)
                                       .ToListAsync();

            // Map each item individually to avoid the collection mapping issue
            return res.Select(product => _mapper.Map<ProductSearchResponse>(product)).ToList();
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


        public async Task<bool> Update(string id, List<ProductVariant> Variants)
        {
            var filter = Builders<Product>.Filter.Eq(x => x.Id, id);
            var update = Builders<Product>.Update.Set(x => x.Variants, Variants);
            var result = await _context.Products.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }
        public async Task<bool> Update(string id, List<VariantOption> Options)
        {
            var filter = Builders<Product>.Filter.Eq(x => x.Id, id);
            var update = Builders<Product>.Update.Set(x => x.Options, Options);
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
                var cloudinaryUrl = await _blobService.UploadToCloudinaryAsync(base64String);
                newImageUrls.Add(cloudinaryUrl);  // Changed from blobClient.AbsoluteUri to cloudinaryUrl
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
        public async Task<List<Product>> GetProductsByProductTypeAsync(ProductType? productType)
        {
            // Create base filter for non-deleted products
            var baseFilter = Builders<Product>.Filter.Eq(x => x.IsDeleted, false);

            // If productType is provided, add it to the filter
            if (productType.HasValue)
            {
                baseFilter = baseFilter & Builders<Product>.Filter.Eq(x => x.productType, productType.Value);
            }

            // Get the base collection directly
            var collection = _context.GetBaseCollection<Product>("Product");

            // Use the base collection to bypass client ID filtering
            var result = await collection.Find(baseFilter)
                .SortByDescending(x => x.CreatedAt)
                .Limit(1000)
                .ToListAsync();

            return result;
        }
        public async Task<Product> GetProductDetailsByIdAsync(string id)
        {
            // Create base filter for non-deleted products with the specific ID
            var filter = Builders<Product>.Filter.Eq(x => x.Id, id) &
                         Builders<Product>.Filter.Eq(x => x.IsDeleted, false);

            // Get the base collection directly to bypass client ID filtering
            var collection = _context.GetBaseCollection<Product>("Product");

            // Use the base collection to retrieve the product
            var result = await collection.Find(filter).FirstOrDefaultAsync();

            return result;
        }
        public async Task<ProductReview> AddProductReviewAsync(string productId, string userId, ServiceInterface.Dtos.product.DMCW.API.Dtos.Product.DMCW.API.Dtos.CreateProductReviewDto reviewDto)
        {
            // Get the user to extract name and profile picture
            var user = await _userService.GetUserByIdAsync(userId);

            if (user == null)
            {
                throw new Exception("User not found");
            }

            // For adding a review, we need to bypass the client filtering to allow customers
            // to review products from any seller
            var baseCollection = _context.GetBaseCollection<Product>("Product");
            var productFilter = Builders<Product>.Filter.Eq(x => x.Id, productId) &
                               Builders<Product>.Filter.Eq(x => x.IsDeleted, false);
            var product = await baseCollection.Find(productFilter).FirstOrDefaultAsync();

            if (product == null)
            {
                throw new Exception("Product not found");
            }

            // Process review images if any
            List<string> reviewImageUrls = new List<string>();
            if (reviewDto.ReviewImages != null && reviewDto.ReviewImages.Count > 0)
            {
                foreach (var base64Image in reviewDto.ReviewImages)
                {
                    var imageUrl = await _blobService.UploadToCloudinaryAsync(base64Image);
                    reviewImageUrls.Add(imageUrl);
                }
            }

            // Create the review
            var review = new ProductReview
            {
                Id = ObjectId.GenerateNewId().ToString(),
                ProductId = productId,
                ReviewerId = userId,
                ReviewerName = user.Name,
                ReviewerProfilePicture = user.ProfilePictureUrl,
                Rating = reviewDto.Rating,
                Comment = reviewDto.Comment,
                ReviewImages = reviewImageUrls,
                LikesCount = 0,
                IsFeatured = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsVerified = false,
                IsDeleted = false
            };

            // Initialize ProductReviews collection if needed
            if (product.ProductReviews == null)
            {
                product.ProductReviews = new List<ProductReview>();
            }

            // Add the review to the product
            product.ProductReviews.Add(review);

            // Update rating statistics
            product.UpdateRatingStats();

            // Update the product using the base collection to bypass client filtering
            var updateResult = await baseCollection.ReplaceOneAsync(productFilter, product);

            if (!updateResult.IsAcknowledged || updateResult.ModifiedCount == 0)
            {
                throw new Exception("Failed to update product with review");
            }

            return review;
        }
        public async Task<List<ProductReview>> GetProductReviewsAsync(string productId)
        {
            // Get the product
            var filter = Builders<Product>.Filter.Eq(x => x.Id, productId);
            var product = await _context.Products.Find(filter).FirstOrDefaultAsync();

            if (product == null || product.ProductReviews == null || product.ProductReviews.Count == 0)
            {
                return new List<ProductReview>();
            }

            // Return only active reviews
            var reviews = product.ProductReviews
                .Where(r => r.IsDeleted != true)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            return reviews;
        }
    }
}