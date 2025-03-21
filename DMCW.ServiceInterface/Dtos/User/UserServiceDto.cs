using System;
using System.Collections.Generic;

namespace DMCW.ServiceInterface.Dtos.User
{
    public class UserServiceDto
    {
        public string? Id { get; set; }
        public string? ClientId { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? UserRole { get; set; } // "Customer", "Seller", or "Admin"
        public AddressDto? Address { get; set; }

        // Seller-specific properties - all nullable
        public SellerProfileDto? SellerProfile { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class StoreDetailsDto
    {
        public string? StoreName { get; set; }
        public string? StoreDescription { get; set; }
    }

    public class AddressDto
    {
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
    }

    public class SellerProfileDto
    {
        public StoreDetailsDto? StoreDetails { get; set; }
        public List<SellerReviewDto>? SellerReviews { get; set; }
        public double? AverageRating { get; set; }
        public bool? IsVerifiedSeller { get; set; }
    }

    public class SellerReviewDto
    {
        public string? Id { get; set; }
        public string? ReviewerId { get; set; }
        public string? ReviewerName { get; set; }
        public string? ReviewerProfilePicture { get; set; }
        public string? SellerId { get; set; }
        public int? Rating { get; set; }
        public string? Comment { get; set; }
        public List<string>? ReviewImages { get; set; }
        public int? LikesCount { get; set; }
        public bool? IsFeatured { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool? IsVerified { get; set; }
        public bool? IsDeleted { get; set; }
    }
}