using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMCW.Repository.Data.Entities.Order
{
    public class LineItem
    {

        [BsonElement("FulfillableQuantity"), BsonRepresentation(BsonType.Int32)]
        public int FulfillableQuantity { get; set; }

        [BsonElement("FulfillmentStatus"), BsonRepresentation(BsonType.String)]
        public string FulfillmentStatus { get; set; }

        [BsonElement("Name"), BsonRepresentation(BsonType.String)]
        public string Name { get; set; }

        [BsonElement("Price"), BsonRepresentation(BsonType.Decimal128)]
        public decimal Price { get; set; }

        [BsonElement("ProductId"), BsonRepresentation(BsonType.String)]
        public string ProductId { get; set; }

        [BsonElement("Quantity"), BsonRepresentation(BsonType.Int32)]
        public int Quantity { get; set; }

        [BsonElement("Title"), BsonRepresentation(BsonType.String)]
        public string Title { get; set; }

        [BsonElement("VariantTitle"), BsonRepresentation(BsonType.String)]
        public string VariantTitle { get; set; }
        [BsonElement("ImageUrl"), BsonRepresentation(BsonType.String)]
        public string ImageUrl { get; set; }

        [BsonElement("VariantId"), BsonRepresentation(BsonType.Int32)]
        public int VariantId { get; set; }

        [BsonElement("SellerId"), BsonRepresentation(BsonType.String)]
        public string SellerId { get; set; }

        [BsonElement("SellerName"), BsonRepresentation(BsonType.String)]
        public string SellerName { get; set; }

        //[BsonElement("TotalDiscount"), BsonRepresentation(BsonType.Decimal128)]
        //public decimal TotalDiscount { get; set; }
    }
}
