using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GarageMasterBE.Models
{
  public class CreateInvoiceRequest
{
    public string CustomerId { get; set; } = null!;
    public string RepairOrderId { get; set; } = null!;
    public string PaymentMethod { get; set; } = null!; // Cash | BankTransfer
}


    }
