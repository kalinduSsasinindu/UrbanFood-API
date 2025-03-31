using DMCW.Repository.Data.Entities.Base;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMCW.Repository.Data.Entities.product
{
    public class Product : BaseEntity
    {
       
        [BsonElement("Title"), BsonRepresentation(BsonType.String)]
        public string Title { get; set; }

        [BsonElement("Description"), BsonRepresentation(BsonType.String)]
        public string Description { get; set; }

        public List<string> Images { get; set; }

        public List<string> ImgUrls { get; set; }

 

        public override string ToString()
        {
            return $"{Title}: {Description}";
        }

        [BsonElement("Tags")]
        public List<string> Tags { get; set; }

     
    }
}
