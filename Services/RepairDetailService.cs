using GarageMasterBE.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace GarageMasterBE.Services
{
  public class RepairDetailService
  {
    private readonly IMongoCollection<RepairDetail> _repairDetails;

    public RepairDetailService(IOptions<MongoDBSettings> mongoSettings, IMongoClient mongoClient)
    {
      var database = mongoClient.GetDatabase(mongoSettings.Value.DatabaseName);
      _repairDetails = database.GetCollection<RepairDetail>("RepairDetails");
    }

    public async Task<List<RepairDetail>> GetAllAsync()
    {
      return await _repairDetails.Find(_ => true).ToListAsync();
    }

    public async Task<RepairDetail?> GetByIdAsync(string id)
    {
      return await _repairDetails.Find(r => r.Id == id).FirstOrDefaultAsync();
    }

    public async Task<RepairDetail> CreateAsync(RepairDetail detail)
    {
      await _repairDetails.InsertOneAsync(detail);
      return detail;
    }

    public async Task<bool> UpdateAsync(string id, RepairDetail updatedDetail)
    {
      var result = await _repairDetails.ReplaceOneAsync(r => r.Id == id, updatedDetail);
      return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
      var result = await _repairDetails.DeleteOneAsync(r => r.Id == id);
      return result.DeletedCount > 0;
    }

    public async Task<List<RepairDetail>> GetByOrderIdAsync(string orderId)
    {
      return await _repairDetails.Find(d => d.RepairOrderId == orderId).ToListAsync();
    }
  }
}