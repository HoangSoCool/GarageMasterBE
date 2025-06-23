using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace GarageMasterBE.Models
{
  public class OrderItem
  {
    [BsonElement("id")]
    public string Id { get; set; } = null!;

    [BsonElement("name")]
    public string Name { get; set; } = null!;

    [BsonElement("price")]
    public decimal Price { get; set; }

    [BsonElement("quantity")]
    public int Quantity { get; set; }

    [BsonElement("image")]
    public string? Image { get; set; }
  }

  public class Order
  {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("userId")]
    public string UserId { get; set; } = null!;

    [BsonElement("customerName")]
    public string CustomerName { get; set; } = null!;

    [BsonElement("phone")]
    public string Phone { get; set; } = null!;

    [BsonElement("address")]
    public string Address { get; set; } = null!;

    [BsonElement("note")]
    public string? Note { get; set; }

    [BsonElement("items")]
    public List<OrderItem> Items { get; set; } = new List<OrderItem>();

    [BsonElement("total")]
    public decimal Total { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  }
}