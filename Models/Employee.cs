using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GarageMasterBE.Models
{
    public class Employee
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = null!;

        [BsonElement("name")]
        public string Name { get; set; } = null!;

        [BsonElement("phone")]
        public string Phone { get; set; } = null!;

        [BsonElement("address")]
        public string Address { get; set; } = null!;

        [BsonElement("employeeRole")]
        [BsonRepresentation(BsonType.String)]
        public EmployeeRole EmployeeRole { get; set; } = EmployeeRole.Employee;

        [BsonElement("dateJoined")]
        public DateTime DateJoined { get; set; } = DateTime.UtcNow;
    }

    public enum EmployeeRole
    {
        Admin,
        Manager,
        Accountant,
        Mechanic,
        Employee
    }
}
