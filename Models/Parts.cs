using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GarageMasterBE.Models
{
    public class Parts
    {
        [BsonId] // Khóa chính trong MongoDB
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } = null!;

        [BsonElement("name")]
        public string Name { get; set; } = null!;

        [BsonElement("quantity")]
        public int Quantity { get; set; }

        [BsonElement("price")]
        public decimal Price { get; set; } // Giá bán cho khách

        [BsonElement("buyPrice")]
        public decimal BuyPrice { get; set; } // Giá nhập vào

        [BsonElement("empPrice")]
        public decimal EmpPrice { get; set; } // Giá bán cho nhân viên

        [BsonElement("unit")]
        public string Unit { get; set; } = null!; // Đơn vị: cái, bộ, lít,...

        [BsonElement("limitStock")]
        public int LimitStock { get; set; } // Giới hạn tồn kho tối thiểu

        [BsonElement("brandId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string BrandId { get; set; } = null!; // Thêm để phân loại theo hãng xe

        [BsonElement("image")]
        public string? Image { get; set; } // Link ảnh phụ tùng
    }
}
