using DMCW.Repository.Data.Entities.Base;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace DMCW.Repository.Data.Entities.product
{
    public class Product : BaseEntity
    {
        public Product()
        {
            Variants = new List<ProductVariant>();
            Options = new List<VariantOption>();
        }

        [BsonElement("Title"), BsonRepresentation(BsonType.String)]
        public string Title { get; set; }

        [BsonElement("Description"), BsonRepresentation(BsonType.String)]
        public string Description { get; set; }

        public List<string> Images { get; set; }

        public List<string> ImgUrls { get; set; }


        public List<ProductVariant> Variants { get; set; }
        public List<VariantOption> Options { get; set; }
        public override string ToString()
        {
            return $"{Title}: {Description}";
        }

        [BsonElement("Tags")]
        public List<string> Tags { get; set; }

     
    }
}
