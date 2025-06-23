using GarageMasterBE.Models;
using MongoDB.Driver;

namespace GarageMasterBE.Services
{
    public class InvoiceService
    {
        private readonly IMongoCollection<Invoice> _invoices;
        private readonly IMongoCollection<Customer> _customers;
        private readonly IMongoCollection<RepairOrder> _repairOrders;

        public InvoiceService(IMongoDatabase database)
        {
            _invoices = database.GetCollection<Invoice>("Invoices");
            _customers = database.GetCollection<Customer>("Customers");
            _repairOrders = database.GetCollection<RepairOrder>("RepairOrders");
        }

        public async Task<Invoice> CreateInvoiceAsync(CreateInvoiceRequest request)
        {
            var customer = await _customers.Find(c => c.Id == request.CustomerId).FirstOrDefaultAsync()
                ?? throw new Exception("Customer not found");

            var repairOrder = await _repairOrders.Find(r => r.Id == request.RepairOrderId).FirstOrDefaultAsync()
                ?? throw new Exception("Repair order not found");

            var invoice = new Invoice
            {
                CustomerId = customer.Id!,
                CustomerName = customer.Name,
                RepairOrderId = repairOrder.Id!,
                CheckIn = repairOrder.CreatedAt,      // ✅ Lấy từ RepairOrder
                CheckOut = DateTime.UtcNow,           // ✅ Thời điểm tạo hóa đơn
                PaymentMethod = request.PaymentMethod,
                TotalCost = repairOrder.TotalCost
            };

            await _invoices.InsertOneAsync(invoice);
            return invoice;
        }


        public async Task<List<Invoice>> GetAllInvoicesAsync()
        {
            return await _invoices.Find(_ => true).ToListAsync();
        }

        public async Task<List<Invoice>> GetInvoicesByCustomerIdAsync(string customerId)
        {
            return await _invoices.Find(i => i.CustomerId == customerId).ToListAsync();
        }
    }
}
