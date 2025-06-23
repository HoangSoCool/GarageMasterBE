using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GarageMasterBE.Models
{
  public class RepairOrder
  {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("customerId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string CustomerId { get; set; } = null!;

    [BsonElement("licensePlate")]
    public string LicensePlate { get; set; } = null!;

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("status")]
    [BsonRepresentation(BsonType.String)]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RepairOrderStatus Status { get; set; } = RepairOrderStatus.Pending;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("totalCost")]
    public decimal TotalCost { get; set; } = 0;

    [BsonElement("employeeId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string EmployeeId { get; set; } = null!;
  }

  public enum RepairOrderStatus
  {
    Pending,        // Đang chờ xử lý
    InProgress,     // Đang sửa
    Completed,      // Đã hoàn thành
    Cancelled       // Đã hủy
  }
}