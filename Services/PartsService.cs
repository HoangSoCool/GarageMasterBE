using GarageMasterBE.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace GarageMasterBE.Services
{
    public class PartsService
    {
        private readonly IMongoCollection<Parts> _partsCollection;
        private readonly IMongoCollection<Brand> _brandsCollection;

           public PartsService(IOptions<MongoDBSettings> mongoSettings, IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase(mongoSettings.Value.DatabaseName);
               _partsCollection = database.GetCollection<Parts>("Parts");
            _brandsCollection = database.GetCollection<Brand>("Brands");
        }


        // Lấy toàn bộ phụ tùng
        public async Task<List<Parts>> GetAllAsync()
        {
            return await _partsCollection.Find(_ => true).ToListAsync();
        }

        // Lấy theo ID
        public async Task<Parts?> GetByIdAsync(string id)
        {
            return await _partsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        // Tạo phụ tùng mới
        public async Task<Parts> CreateAsync(Parts part)
        {
            if (!await IsValidBrandId(part.BrandId))
                throw new Exception("Hãng xe không hợp lệ.");

            await _partsCollection.InsertOneAsync(part);
            return part;
        }

        // Cập nhật phụ tùng
        public async Task<bool> UpdateAsync(string id, Parts updatedPart)
        {
            if (!await IsValidBrandId(updatedPart.BrandId))
                throw new Exception("Hãng xe không hợp lệ.");

            var result = await _partsCollection.ReplaceOneAsync(x => x.Id == id, updatedPart);
            return result.ModifiedCount > 0;
        }

        // Xóa phụ tùng
        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _partsCollection.DeleteOneAsync(x => x.Id == id);
            return result.DeletedCount > 0;
        }

        // Kiểm tra BrandId có hợp lệ không
        private async Task<bool> IsValidBrandId(string brandId)
        {
            return await _brandsCollection.Find(x => x.Id == brandId).AnyAsync();
        }

        // Lấy phụ tùng theo BrandId (nếu cần)
        public async Task<List<Parts>> GetByBrandIdAsync(string brandId)
        {
            return await _partsCollection.Find(p => p.BrandId == brandId).ToListAsync();
        }
        // Lấy phụ tùng theo name (nếu cần)
        public async Task<List<Parts>> GetByNameAsync(string name)
        {
            var filter = Builders<Parts>.Filter.Regex(
                p => p.Name, 
                new MongoDB.Bson.BsonRegularExpression(name, "i")
            );
            return await _partsCollection.Find(filter).ToListAsync();
        }

    }
}
