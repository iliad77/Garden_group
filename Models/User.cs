using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Garden_group.Models
{
    
    
    public class User
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("name")]
        public Name Name { get; set; } = new Name();

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("phone")]
        public string Phone { get; set; } = string.Empty;

        [BsonElement("address")]
        public string Address { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.String)]
        [BsonElement("userType")]
        public UserType UserType { get; set; }

        [BsonElement("passwordHash")] 
        public string Password { get; set; }

    }

    public class Name
    {
        [BsonElement("first")]
        public string First { get; set; } = string.Empty;
        [BsonElement("last")]
        public string Last { get; set; } = string.Empty;

    }
}
