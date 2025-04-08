using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace DMCW.Repository.Data.Entities.product
{
    public class ProductReview
    {
        [BsonElement("_id")]
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("ProductId")]
        public string? ProductId { get; set; }

        [BsonElement("ReviewerId")]
        public string? ReviewerId { get; set; }

        [BsonElement("ReviewerName")]
        public string? ReviewerName { get; set; }

        [BsonElement("ReviewerProfilePicture")]
        public string? ReviewerProfilePicture { get; set; }

        [BsonElement("Rating")]
        public int? Rating { get; set; }

        [BsonElement("Comment")]
        public string? Comment { get; set; }

        [BsonElement("ReviewImages")]
        public List<string>? ReviewImages { get; set; }

        [BsonElement("LikesCount")]
        public int? LikesCount { get; set; }

        [BsonElement("IsFeatured")]
        public bool? IsFeatured { get; set; }

        [BsonElement("CreatedAt")]
        public DateTime? CreatedAt { get; set; }

        [BsonElement("UpdatedAt")]
        public DateTime? UpdatedAt { get; set; }

        [BsonElement("IsVerified")]
        public bool? IsVerified { get; set; }

        [BsonElement("IsDeleted")]
        public bool? IsDeleted { get; set; }
    }
}