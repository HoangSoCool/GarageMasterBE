using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GarageMasterBE.Models
{
    public class Moto
    {
        [BsonId] // Sử dụng biển số làm khóa chính
        public string LicensePlate { get; set; } = null!;

        [BsonElement("brandId")]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string BrandId { get; set; } = null!; // Tham chiếu bảng Brand (_id)

        [BsonElement("model")]
        public string Model { get; set; } = null!;

        [BsonElement("customerId")]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string CustomerId { get; set; } = null!;

        [BsonElement("dateOfSent")]
        public DateTime DateOfSent { get; set; } = DateTime.Now;

        [BsonElement("notes")]
        public string? Notes { get; set; }
    }
}
