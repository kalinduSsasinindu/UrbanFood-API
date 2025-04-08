using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMCW.Repository.Data.Entities.Order
{
    public class ShippingAddress
    {

        [BsonElement("FirstName"), BsonRepresentation(BsonType.String)]
        public string FirstName { get; set; }
        [BsonElement("LastName"), BsonRepresentation(BsonType.String)]
        public string LastName { get; set; }
        [BsonElement("Phone"), BsonRepresentation(BsonType.String)]
        public string Phone { get; set; }

        [BsonElement("Address1"), BsonRepresentation(BsonType.String)]
        public string Address1 { get; set; }
        [BsonElement("Address2"), BsonRepresentation(BsonType.String)]
        public string Address2 { get; set; }
        public string City { get; set; }

    }
}
