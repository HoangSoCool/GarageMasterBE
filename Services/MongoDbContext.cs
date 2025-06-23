using GarageMasterBE.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GarageMasterBE.Services
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        private readonly ILogger<MongoDbContext> _logger;

        public MongoDbContext(IOptions<MongoDBSettings> settings, ILogger<MongoDbContext> logger)
        {
            _logger = logger;

            try
            {
                var client = new MongoClient(settings.Value.ConnectionString);
                _database = client.GetDatabase(settings.Value.DatabaseName);

                // Gửi lệnh ping kiểm tra kết nối
                var command = new BsonDocument("ping", 1);
                _database.RunCommand<BsonDocument>(command);

                _logger.LogInformation("Kết nối MongoDB thành công tới database: {DatabaseName}", settings.Value.DatabaseName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kết nối MongoDB thất bại");
                throw; // tùy chọn: có thể throw hoặc không tùy mục đích
            }
        }

        public IMongoDatabase Database => _database;
    }
}
