using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMCW.Repository.Data.Entities.Customer
{
    public class CustomerInfo
    {
        [BsonElement("Email"), BsonRepresentation(BsonType.String)]
        public string Email { get; set; }
        [BsonElement("FirstName"), BsonRepresentation(BsonType.String)]
        public string FirstName { get; set; }
        [BsonElement("LastName"), BsonRepresentation(BsonType.String)]
        public string LastName { get; set; }
        [BsonElement("Phone"), BsonRepresentation(BsonType.String)]
        public string Phone { get; set; }

       

    }
}
