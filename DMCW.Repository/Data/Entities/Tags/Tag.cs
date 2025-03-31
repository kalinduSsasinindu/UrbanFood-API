using DMCW.Repository.Data.Entities.Base;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMCW.Repository.Data.Entities.Tags
{
    public class Tag : BaseEntity
    {
        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("kind")]
        public string Kind { get; set; }
    }
}
