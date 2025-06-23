using GarageMasterBE.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace GarageMasterBE.Services
{
  public class RepairOrderService
  {
    private readonly IMongoCollection<RepairOrder> _repairOrders;

    public RepairOrderService(IOptions<MongoDBSettings> mongoSettings, IMongoClient mongoClient)
    {
      var database = mongoClient.GetDatabase(mongoSettings.Value.DatabaseName);
      _repairOrders = database.GetCollection<RepairOrder>("RepairOrders");
    }

    public async Task<List<RepairOrder>> GetAllAsync()
    {
      return await _repairOrders.Find(_ => true).ToListAsync();
    }

    public async Task<RepairOrder?> GetByIdAsync(string id)
    {
      return await _repairOrders.Find(r => r.Id == id).FirstOrDefaultAsync();
    }

    public async Task<RepairOrder> CreateAsync(RepairOrder order)
    {
      await _repairOrders.InsertOneAsync(order);
      return order;
    }

    public async Task<bool> UpdateAsync(string id, RepairOrder updatedOrder)
    {
      var result = await _repairOrders.ReplaceOneAsync(r => r.Id == id, updatedOrder);
      return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
      var result = await _repairOrders.DeleteOneAsync(r => r.Id == id);
      return result.DeletedCount > 0;
    }

    // Tìm kiếm theo mã đơn hoặc tên khách hàng (ví dụ)
  }
}