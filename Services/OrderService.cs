using GarageMasterBE.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace GarageMasterBE.Services
{
  public class OrderService
  {
    private readonly IMongoCollection<Order> _orders;

    public OrderService(IOptions<MongoDBSettings> mongoSettings, IMongoClient mongoClient)
    {
      var database = mongoClient.GetDatabase(mongoSettings.Value.DatabaseName);
      _orders = database.GetCollection<Order>("Orders");
    }

    public async Task<Order> CreateAsync(Order order)
    {
      await _orders.InsertOneAsync(order);
      return order;
    }

    public async Task<List<Order>> GetByUserIdAsync(string userId)
    {
      return await _orders.Find(o => o.UserId == userId).ToListAsync();
    }

    public async Task<List<Order>> GetAllAsync()
    {
      return await _orders.Find(_ => true).ToListAsync();
    }

    public async Task<List<Order>> GetByUserAsync(string userId)
    {
      return await _orders.Find(o => o.UserId == userId).ToListAsync();
    }
  }
}