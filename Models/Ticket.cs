using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Garden_group.Models
{
    // Maps to a document in the tickets collection
    public class Ticket
    {
        // Null for a new ticket, MongoDB generates the ObjectId on insert
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("subject")]
        public string Subject { get; set; } = string.Empty;

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("incidentType")]
        [BsonRepresentation(BsonType.String)]
        public IncidentType IncidentType { get; set; }

        [BsonElement("priority")]
        [BsonRepresentation(BsonType.String)]
        public TicketPriority Priority { get; set; }

        // New tickets always start as Open
        [BsonElement("status")]
        [BsonRepresentation(BsonType.String)]
        public TicketStatus Status { get; set; } = TicketStatus.Open;

        [BsonElement("timeReported")]
        public DateTime TimeReported { get; set; }

        [BsonElement("deadlineFollowUp")]
        public DateTime DeadlineFollowUp { get; set; }

        // Reference to users._id
        [BsonElement("reportedBy")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ReportedBy { get; set; } = string.Empty;

        // Empty until a service desk employee picks the ticket up
        [BsonElement("handledBy")]
        public List<TicketHandling> HandledBy { get; set; } = new();
    }
}
