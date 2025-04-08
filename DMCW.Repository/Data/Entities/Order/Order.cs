using DMCW.Repository.Data.Entities.Base;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMCW.Repository.Data.Entities.Customer;

namespace DMCW.Repository.Data.Entities.Order
{
    public class Order : BaseEntity
    {
        [BsonElement("FinancialStatus"), BsonRepresentation(BsonType.String)]
        public string FinancialStatus { get; set; }

        [BsonElement("FulfillmentStatus"), BsonRepresentation(BsonType.String)]
        public string FulfillmentStatus { get; set; }

        [BsonElement("Name"), BsonRepresentation(BsonType.String)]
        public string Name { get; set; }

        [BsonElement("Note"), BsonRepresentation(BsonType.String)]
        public string Note { get; set; }

        [BsonElement("PaymentMethod"), BsonRepresentation(BsonType.String)]
        public string[] PaymentMethod { get; set; }

        [BsonElement("Phone"), BsonRepresentation(BsonType.String)]
        public string Phone { get; set; }

        [BsonElement("SubtotalPrice"), BsonRepresentation(BsonType.Decimal128)]
        public decimal SubtotalPrice { get; set; }

        [BsonElement("TotalLineItemsPrice"), BsonRepresentation(BsonType.Decimal128)]
        public decimal TotalLineItemsPrice { get; set; }

        [BsonElement("TotalOutstanding"), BsonRepresentation(BsonType.Decimal128)]
        public decimal TotalOutstanding => TotalPrice - (PaymentInfo?.TotalPaidAmount) ?? 0;

        [BsonElement("TotalPrice"), BsonRepresentation(BsonType.Decimal128)]
        public decimal TotalPrice { get; set; }

        [BsonElement("TotalShippingPrice"), BsonRepresentation(BsonType.Decimal128)]
        public decimal TotalShippingPrice { get; set; }
        public decimal TotalDiscountPrice { get; set; }
        public ShippingAddress ShippingAddress { get; set; }
        public CustomerInfo Customer { get; set; }
        public List<LineItem> LineItems { get; set; } = new List<LineItem>();
        public PaymentInfo PaymentInfo { get; set; }
        [BsonElement("isCancelled")]
        public bool IsCancelled { get; set; }
        public List<TimeLineDetails> TimeLineDetails { get; set; }
        [BsonElement("Tags")]
        public List<string> Tags { get; set; }
    }
}
