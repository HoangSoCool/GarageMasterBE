using GarageMasterBE.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace GarageMasterBE.Services
{
  public class ReviewService
  {
    private readonly IMongoCollection<Review> _reviews;

    public ReviewService(IOptions<MongoDBSettings> mongoSettings, IMongoClient mongoClient)
    {
      var database = mongoClient.GetDatabase(mongoSettings.Value.DatabaseName);
      _reviews = database.GetCollection<Review>("Reviews");
    }

    public async Task<List<Review>> GetAllAsync()
    {
      return await _reviews.Find(_ => true).SortByDescending(r => r.CreatedAt).ToListAsync();
    }

    public async Task<Review?> GetByUserIdAsync(string userId)
    {
      return await _reviews.Find(r => r.UserId == userId).FirstOrDefaultAsync();
    }

    public async Task CreateOrUpdateAsync(Review review)
    {
      var existing = await GetByUserIdAsync(review.UserId);
      if (existing != null)
      {
        review.Id = existing.Id;
        await _reviews.ReplaceOneAsync(r => r.Id == existing.Id, review);
      }
      else
      {
        await _reviews.InsertOneAsync(review);
      }
    }
  }
}