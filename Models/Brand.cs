using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GarageMasterBE.Models
{
    public class Brand
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } = null!;

        [BsonElement("name")]
        public string Name { get; set; } = null!;

        [BsonElement("image")]
        public string? Image { get; set; }
    }
}
