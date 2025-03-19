using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;


namespace DMCW.Repository.Data.Entities.Base
{
    public class BaseEntity : IUserOwnedEntity
    {
        public static class BaseAttributes
        {
            public const string Id = "_id";
            public const string CreatedAt = "CreatedAt";
            public const string UpdatedAt = "UpdatedAt";
            public const string DeletedAt = "DeletedAt";
            public const string IsDeleted = "isDeleted";
            public const string ClientId = "ClientId";
        }

        [BsonId]
        [BsonElement(BaseAttributes.Id), BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement(BaseAttributes.CreatedAt)]
        public DateTime CreatedAt { get; set; }
        [BsonElement(BaseAttributes.UpdatedAt)]
        public DateTime UpdatedAt { get; set; }
        [BsonElement(BaseAttributes.DeletedAt)]
        public DateTime DeletedAt { get; set; }
        [BsonElement(BaseAttributes.IsDeleted)]
        public bool IsDeleted { get; set; }
        [BsonElement(BaseAttributes.ClientId)]
        public string? ClientId { get; set; }

        public BaseEntity()
        {
            Id = ObjectId.GenerateNewId().ToString();
            CreatedAt = DateTime.UtcNow;
        }
    }
}
