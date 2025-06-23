using GarageMasterBE.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace GarageMasterBE.Services
{
    public class CustomerService
    {
        private readonly IMongoCollection<Customer> _customersCollection;

        public CustomerService(IOptions<MongoDBSettings> mongoSettings, IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase(mongoSettings.Value.DatabaseName);
            _customersCollection = database.GetCollection<Customer>("Customers");
        }

        // Lấy toàn bộ khách hàng
        public async Task<List<Customer>> GetAllAsync()
        {
            return await _customersCollection.Find(_ => true).ToListAsync();
        }

        // Lấy khách hàng theo ID   
        public async Task<Customer?> GetByIdAsync(string id)
        {
            return await _customersCollection.Find(c => c.Id == id).FirstOrDefaultAsync();
        }

        // Thêm mới khách hàng
        public async Task<Customer> CreateAsync(Customer customer)
        {
            await _customersCollection.InsertOneAsync(customer);
            return customer;
        }

        // Cập nhật thông tin khách hàng
        public async Task<bool> UpdateAsync(string id, Customer updatedCustomer)
        {
            var result = await _customersCollection.ReplaceOneAsync(c => c.Id == id, updatedCustomer);
            return result.ModifiedCount > 0;
        }

        // Xóa khách hàng
        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _customersCollection.DeleteOneAsync(c => c.Id == id);
            return result.DeletedCount > 0;
        }

        // Tìm kiếm khách hàng theo tên (partial, không phân biệt hoa thường)
        public async Task<List<Customer>> GetByNameAsync(string name)
        {
            var filter = Builders<Customer>.Filter.Regex(
                c => c.Name,
                new MongoDB.Bson.BsonRegularExpression(name, "i")
            );
            return await _customersCollection.Find(filter).ToListAsync();
        }

        // Tìm kiếm khách hàng theo email chính xác
    }
}
