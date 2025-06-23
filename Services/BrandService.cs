using GarageMasterBE.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace GarageMasterBE.Services
{
    public class BrandService
    {
        private readonly IMongoCollection<Brand> _brandsCollection;

        public BrandService(IOptions<MongoDBSettings> mongoSettings, IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase(mongoSettings.Value.DatabaseName);
            _brandsCollection = database.GetCollection<Brand>("Brands");
        }

        // Lấy toàn bộ hãng xe
        public async Task<List<Brand>> GetAllAsync()
        {
            return await _brandsCollection.Find(_ => true).ToListAsync();
        }

        // Lấy hãng theo ID
        public async Task<Brand?> GetByIdAsync(string id)
        {
            return await _brandsCollection.Find(b => b.Id == id).FirstOrDefaultAsync();
        }

        // Tạo hãng mới
        public async Task<Brand> CreateAsync(Brand brand)
        {
            await _brandsCollection.InsertOneAsync(brand);
            return brand;
        }

        // Cập nhật hãng
        public async Task<bool> UpdateAsync(string id, Brand updatedBrand)
        {
            var result = await _brandsCollection.ReplaceOneAsync(b => b.Id == id, updatedBrand);
            return result.ModifiedCount > 0;
        }

        // Xóa hãng
        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _brandsCollection.DeleteOneAsync(b => b.Id == id);
            return result.DeletedCount > 0;
        }

        // Tìm hãng theo tên (partial, case-insensitive)
        public async Task<List<Brand>> GetByNameAsync(string name)
        {
            var filter = Builders<Brand>.Filter.Regex(
                b => b.Name, 
                new MongoDB.Bson.BsonRegularExpression(name, "i")
            );
            return await _brandsCollection.Find(filter).ToListAsync();
        }
    }
}
