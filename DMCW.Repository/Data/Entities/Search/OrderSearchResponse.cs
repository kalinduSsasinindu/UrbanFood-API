using DMCW.Repository.Data.Entities.Order;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMCW.Repository.Data.Entities.Search
{
    public class OrderSearchResponse
    {
        [BsonId]
        [BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string FinancialStatus { get; set; }
        public string FulfillmentStatus { get; set; }
        public string Name { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal TotalOutstanding { get; set; }
        public ShippingAddress ShippingAddress { get; set; }
        public List<string> TrackingIds { get; set; }
        public string TrackingError { get; set; }
        public string DeliveryStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public int lineItemCount { get; set; }
        public bool IsCancelled { get; set; }
        public string? ClientId { get; set; }
        public int? IsExchange { get; set; }
    }
}
