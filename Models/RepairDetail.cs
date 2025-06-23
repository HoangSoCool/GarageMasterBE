using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GarageMasterBE.Models
{
  public class RepairDetail
  {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("repairOrderId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string RepairOrderId { get; set; } = null!;  // Liên kết đến RepairOrder

    [BsonElement("partId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string PartId { get; set; } = null!;   // Liên kết đến Part

    [BsonElement("quantity")]
    public int Quantity { get; set; }

    [BsonElement("unitPrice")]
    public decimal UnitPrice { get; set; }

    [BsonElement("totalPrice")]
    public decimal TotalPrice => Quantity * UnitPrice;
  }
}