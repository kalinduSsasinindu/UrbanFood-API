using DMCW.Repository.Data.Entities.Base;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using DMCW.Shared.Utility.Enums;

namespace DMCW.Repository.Data.Entities.product
{
    public class Product : BaseEntity
    {
        public Product()
        {
            Variants = new List<ProductVariant>();
            Options = new List<VariantOption>();
            ProductReviews = new List<ProductReview>();
            ImgUrls = new List<string>();
            Images = new List<string>();
            Tags = new List<string>();
        }

        [BsonElement("Title"), BsonRepresentation(BsonType.String)]
        public string Title { get; set; }

        [BsonElement("Description"), BsonRepresentation(BsonType.String)]
        public string Description { get; set; }

        public List<string> Images { get; set; }

        public List<string> ImgUrls { get; set; }

        public ProductType productType { get; set; }

        public List<ProductVariant> Variants { get; set; }

        public List<VariantOption> Options { get; set; }

        [BsonElement("Tags")]
        public List<string> Tags { get; set; }

        [BsonElement("ProductReviews")]
        public List<ProductReview>? ProductReviews { get; set; }

        [BsonElement("AverageRating")]
        public double? AverageRating { get; set; }

        [BsonElement("ReviewCount")]
        public int? ReviewCount { get; set; }

        [BsonElement("FeaturedReviewId")]
        public string? FeaturedReviewId { get; set; }

        public void UpdateRatingStats()
        {
            if (ProductReviews == null || ProductReviews.Count == 0 || !ProductReviews.Any(r => r.IsDeleted != true))
            {
                AverageRating = 0;
                ReviewCount = 0;
                return;
            }

            var activeReviews = ProductReviews.Where(r => r.IsDeleted != true).ToList();
            ReviewCount = activeReviews.Count;
            AverageRating = activeReviews.Average(r => r.Rating) ?? 0;
        }

        public override string ToString()
        {
            return $"{Title}: {Description}";
        }
    }
}