using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Net;

namespace DMCW.Repository.Data.Entities.User
{
    public class User
    {
        [BsonId]
        [BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("ClientId")]
        public string? ClientId { get; set; }

        [BsonElement("Email")]
        public string? Email { get; set; }

        [BsonElement("Name")]
        public string? Name { get; set; }

        [BsonElement("Phone")]
        public string? Phone { get; set; }

        [BsonElement("ProfilePictureUrl")]
        public string? ProfilePictureUrl { get; set; }

        [BsonElement("UserRole")]
        public string? UserRole { get; set; } // "Customer", "Seller", or "Admin"

        [BsonElement("Address")]
        public Address? Address { get; set; }

        [BsonElement("SellerProfile")]
        public SellerProfile? SellerProfile { get; set; }

        [BsonElement("CreatedAt")]
        public DateTime? CreatedAt { get; set; }

        [BsonElement("UpdatedAt")]
        public DateTime? UpdatedAt { get; set; }

        [BsonElement("LastLoginDate")]
        public DateTime? LastLoginDate { get; set; }

        [BsonElement("IsActive")]
        public bool? IsActive { get; set; }

        [BsonElement("IsDeleted")]
        public bool? IsDeleted { get; set; }
    }

    public class SellerProfile
    {
        [BsonElement("StoreDetails")]
        public StoreDetails? StoreDetails { get; set; }

        [BsonElement("SellerReviews")]
        public List<SellerReview>? SellerReviews { get; set; }

        [BsonElement("AverageRating")]
        public double? AverageRating { get; set; }

        [BsonElement("IsVerifiedSeller")]
        public bool? IsVerifiedSeller { get; set; }
    }

    public class Address
    {
        [BsonElement("Street")]
        public string? Street { get; set; }

        [BsonElement("City")]
        public string? City { get; set; }

        [BsonElement("State")]
        public string? State { get; set; }

        [BsonElement("ZipCode")]
        public string? ZipCode { get; set; }
    }

    public class StoreDetails
    {
        [BsonElement("StoreName")]
        public string? StoreName { get; set; }

        [BsonElement("StoreDescription")]
        public string? StoreDescription { get; set; }
    }

    public class SellerReview
    {
        [BsonElement("_id")]
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("ReviewerId")]
        public string? ReviewerId { get; set; }

        [BsonElement("ReviewerName")]
        public string? ReviewerName { get; set; }

        [BsonElement("ReviewerProfilePicture")]
        public string? ReviewerProfilePicture { get; set; }

        [BsonElement("SellerId")]
        public string? SellerId { get; set; }

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