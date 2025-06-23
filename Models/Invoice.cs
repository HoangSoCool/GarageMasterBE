using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GarageMasterBE.Models
{
    public enum PaymentMethod
    {
        Cash,          // Tiền mặt
        BankTransfer   // Chuyển khoản
    }

    public class Invoice
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("customerId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string CustomerId { get; set; } = null!;

    [BsonElement("customerName")]
    public string CustomerName { get; set; } = null!;

    [BsonElement("repairOrderId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string RepairOrderId { get; set; } = null!;

    [BsonElement("checkIn")]
    public DateTime CheckIn { get; set; } // Lấy từ RepairOrder.CreatedAt

    [BsonElement("checkOut")]
    public DateTime CheckOut { get; set; } = DateTime.UtcNow; // Lúc tạo Invoice

    [BsonElement("paymentMethod")]
    public string PaymentMethod { get; set; } = null!; // Cash | BankTransfer

    [BsonElement("totalCost")]
    public decimal TotalCost { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

}
