// Author: Luca Mulders
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Garden_group.Models
{
    // One action by a service desk employee, embedded in a ticket's handledBy list
    public class TicketHandling
    {
        // Reference to users._id
        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("handledAt")]
        public DateTime HandledAt { get; set; }

        [BsonElement("comment")]
        public string Comment { get; set; } = string.Empty;
    }
}
