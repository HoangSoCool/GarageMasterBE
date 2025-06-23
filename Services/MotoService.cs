using GarageMasterBE.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace GarageMasterBE.Services
{
    public class MotoService
    {
        private readonly IMongoCollection<Moto> _motoCollection;

        public MotoService(IOptions<MongoDBSettings> mongoSettings, IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase(mongoSettings.Value.DatabaseName);
            _motoCollection = database.GetCollection<Moto>("Motos");
        }

               public async Task<List<Moto>> GetAllAsync()
        {
            return await _motoCollection.Find(_ => true).ToListAsync();
        }

              public async Task<Moto?> GetByLicensePlateAsync(string licensePlate)
        {
            return await _motoCollection.Find(m => m.LicensePlate == licensePlate).FirstOrDefaultAsync();
        }

       
        public async Task<Moto> CreateAsync(Moto moto)
        {
            await _motoCollection.InsertOneAsync(moto);
            return moto;
        }

      
        public async Task<bool> UpdateAsync(string licensePlate, Moto updatedMoto)
        {
            var result = await _motoCollection.ReplaceOneAsync(m => m.LicensePlate == licensePlate, updatedMoto);
            return result.ModifiedCount > 0;
        }

      
        public async Task<bool> DeleteAsync(string licensePlate)
        {
            var result = await _motoCollection.DeleteOneAsync(m => m.LicensePlate == licensePlate);
            return result.DeletedCount > 0;
        }

       
        public async Task<List<Moto>> GetByCustomerIdAsync(string customerId)
        {
            return await _motoCollection.Find(m => m.CustomerId == customerId).ToListAsync();
        }
    }
}
