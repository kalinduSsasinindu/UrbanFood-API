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
        public string Id { get; set; }

        [BsonElement("GoogleId")]
        public string GoogleId { get; set; }

        [BsonElement("ClientId")]
        public string? ClientId { get; set; }

        [BsonElement("Email")]
        public string Email { get; set; }

        [BsonElement("Name")]
        public string Name { get; set; }

        [BsonElement("Phone")]
        public string Phone { get; set; }

        [BsonElement("ProfilePictureUrl")]
        public string ProfilePictureUrl { get; set; }

        [BsonElement("UserRole")]
        public string UserRole { get; set; } // "Customer", "Seller", or "Admin"

        [BsonElement("Address")]
        public Address Address { get; set; }

        // Seller-specific properties
        [BsonElement("StoreName")]
        public string StoreName { get; set; }

        [BsonElement("StoreDescription")]
        public string StoreDescription { get; set; }

        [BsonElement("BusinessHours")]
        public Dictionary<string, BusinessHours> BusinessHours { get; set; }

        [BsonElement("IsVerifiedSeller")]
        public bool? IsVerifiedSeller { get; set; }

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("UpdatedAt")]
        public DateTime UpdatedAt { get; set; }

        [BsonElement("LastLoginDate")]
        public DateTime? LastLoginDate { get; set; }

        [BsonElement("IsEmailVerified")]
        public bool IsEmailVerified { get; set; }

        [BsonElement("IsActive")]
        public bool IsActive { get; set; } = true;

        [BsonElement("IsDeleted")]
        public bool IsDeleted { get; set; }
    }

    public class Address
    {
        [BsonElement("Street")]
        public string Street { get; set; }

        [BsonElement("City")]
        public string City { get; set; }

        [BsonElement("State")]
        public string State { get; set; }

        [BsonElement("ZipCode")]
        public string ZipCode { get; set; }

        [BsonElement("Coordinates")]
        public GeoCoordinates Coordinates { get; set; }
    }

    public class GeoCoordinates
    {
        [BsonElement("Latitude")]
        public double Latitude { get; set; }

        [BsonElement("Longitude")]
        public double Longitude { get; set; }
    }

    public class BusinessHours
    {
        [BsonElement("Open")]
        public string OpenTime { get; set; }

        [BsonElement("Close")]
        public string CloseTime { get; set; }

        [BsonElement("IsClosed")]
        public bool IsClosed { get; set; }
    }
}