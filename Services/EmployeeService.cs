using GarageMasterBE.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace GarageMasterBE.Services
{
    public class EmployeeService
    {
        private readonly IMongoCollection<Employee> _employeesCollection;

        public EmployeeService(IOptions<MongoDBSettings> mongoSettings, IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase(mongoSettings.Value.DatabaseName);
            _employeesCollection = database.GetCollection<Employee>("Employees");
        }

        // Lấy tất cả nhân viên
        public async Task<List<Employee>> GetAllAsync()
        {
            return await _employeesCollection.Find(_ => true).ToListAsync();
        }

        // Lấy nhân viên theo Id
        public async Task<Employee?> GetByIdAsync(string id)
        {
            return await _employeesCollection.Find(e => e.Id == id).FirstOrDefaultAsync();
        }

        // Lấy nhân viên theo UserId
        public async Task<Employee?> GetByUserIdAsync(string userId)
        {
            return await _employeesCollection.Find(e => e.UserId == userId).FirstOrDefaultAsync();
        }

        // Tạo mới nhân viên
        public async Task<Employee> CreateAsync(Employee employee)
        {
            await _employeesCollection.InsertOneAsync(employee);
            return employee;
        }

        // Cập nhật thông tin nhân viên
        public async Task<bool> UpdateAsync(string id, Employee updatedEmployee)
        {
            var result = await _employeesCollection.ReplaceOneAsync(e => e.Id == id, updatedEmployee);
            return result.ModifiedCount > 0;
        }

        // Xóa nhân viên
        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _employeesCollection.DeleteOneAsync(e => e.Id == id);
            return result.DeletedCount > 0;
        }

        // Tìm kiếm theo tên (partial match, không phân biệt hoa thường)
        public async Task<List<Employee>> SearchByNameAsync(string name)
        {
            var filter = Builders<Employee>.Filter.Regex(e => e.Name, new MongoDB.Bson.BsonRegularExpression(name, "i"));
            return await _employeesCollection.Find(filter).ToListAsync();
        }
    }
}
