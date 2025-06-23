using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;

namespace GarageMasterBE.Models
{
      public class UpdateUserRoleRequest
    {
        [Required]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string UserId { get; set; } = null!;

        [Required]
        public string Role { get; set; } = null!;

        [Required]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string LinkedEntityId { get; set; } = null!;
    }

}