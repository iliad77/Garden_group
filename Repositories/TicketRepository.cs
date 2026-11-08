// Author: Luca Mulders + Lera Shorin
using Garden_group.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Garden_group.Repositories
{
    public class TicketRepository : ITicketRepository
    {
        private readonly IMongoCollection<Ticket> _tickets;

        public TicketRepository(IMongoDatabase database)
        {
            _tickets = database.GetCollection<Ticket>("tickets");

            // Speeds up "my tickets" and the per-user dashboard, does nothing if the index already exists
            _tickets.Indexes.CreateOne(new CreateIndexModel<Ticket>(
                Builders<Ticket>.IndexKeys.Ascending(t => t.ReportedBy)));
        }

        /// <inheritdoc/>
        public async Task<List<Ticket>> GetAllAsync()
        {
            return await _tickets.Find(FilterDefinition<Ticket>.Empty)
                .SortByDescending(t => t.TimeReported)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<List<Ticket>> GetByUserAsync(string userId)
        {
            if (!ObjectId.TryParse(userId, out _))
                return new List<Ticket>();

            return await _tickets.Find(t => t.ReportedBy == userId)
                .SortByDescending(t => t.TimeReported)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<Ticket?> GetByIdAsync(string id)
        {
            // An invalid id string would make the driver throw, so treat it as not found
            if (!ObjectId.TryParse(id, out _))
                return null;

            return await _tickets.Find(t => t.Id == id).FirstOrDefaultAsync();
        }

        /// <inheritdoc/>
        public async Task<Ticket> CreateAsync(Ticket ticket)
        {
            ticket.Id = null;
            ticket.Status = TicketStatus.Open;
            ticket.HandledBy = new List<TicketHandling>();
            ticket.Owner = null;

            await _tickets.InsertOneAsync(ticket);
            return ticket;
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateAsync(Ticket ticket)
        {
            if (!ObjectId.TryParse(ticket.Id, out _))
                return false;

            var result = await _tickets.ReplaceOneAsync(t => t.Id == ticket.Id, ticket);
            return result.MatchedCount > 0;
        }

        /// <inheritdoc/>
        public async Task<bool> ChangeStatusAsync(string id, TicketStatus status, TicketHandling handling)
        {
            if (!ObjectId.TryParse(id, out _))
                return false;

            // $set and $push combined so the status and its history entry are saved in one round trip
            var update = Builders<Ticket>.Update
                .Set(t => t.Status, status)
                .Push(t => t.HandledBy, handling);

            var result = await _tickets.UpdateOneAsync(t => t.Id == id, update);
            return result.MatchedCount > 0;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(string id)
        {
            if (!ObjectId.TryParse(id, out _))
                return false;

            var result = await _tickets.DeleteOneAsync(t => t.Id == id);
            return result.DeletedCount > 0;
        }

        /// <inheritdoc/>
        public async Task<Dictionary<TicketStatus, int>> GetStatusCountsAsync(string? userId = null)
        {
            // Every status starts at 0 so the dashboard also shows statuses with no tickets
            var counts = Enum.GetValues<TicketStatus>().ToDictionary(s => s, s => 0);
            if (userId != null && !ObjectId.TryParse(userId, out _))
                return counts;

            var filter = userId == null
                ? FilterDefinition<Ticket>.Empty
                : Builders<Ticket>.Filter.Eq(t => t.ReportedBy, userId);

            // Aggregation pipeline: $match on the user (optional), then $group by status and count
            var groups = await _tickets.Aggregate()
                .Match(filter)
                .Group(t => t.Status, g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var group in groups)
                counts[group.Status] = group.Count;

            return counts;
        }


        /// <inheritdoc/>
        public async Task<Dictionary<ObjectId, int>> GetOpenTicketCountPerAssigneeAsync()
        {
            // "Assignee" = last entry in handledBy, because ChangeStatusAsync pushes every new
            // handling to the end of the array.
            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument
                {
                    { "status", new BsonDocument("$nin", new BsonArray
                        {
                            TicketStatus.Resolved.ToString(),
                            TicketStatus.Closed.ToString()
                        }) },
                    { "owner", new BsonDocument("$ne", BsonNull.Value) }
                }),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$owner" },
                    { "count", new BsonDocument("$sum", 1) }
                })
            };

            var results = await _tickets.Aggregate<BsonDocument>(pipeline).ToListAsync();

            return results.ToDictionary(d => d["_id"].AsObjectId, d => d["count"].AsInt32);
        }


        // adding an owner to ticket
        public async Task<bool> AssignOwnerAsync(string ticketId, string userId)
        {
            if (!ObjectId.TryParse(ticketId, out _) || !ObjectId.TryParse(userId, out _))
                return false;

            var update = Builders<Ticket>.Update.Set(t => t.Owner, userId);
            var result = await _tickets.UpdateOneAsync(t => t.Id == ticketId, update);
            return result.MatchedCount > 0;
        }


        /// <inheritdoc/>
        public async Task<Dictionary<ObjectId, int>> GetCreatedTicketCountPerUserAsync()
        {
            var pipeline = new[]
            {
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$reportedBy" },
                    { "count", new BsonDocument("$sum", 1) }
                })
            };

            var results = await _tickets.Aggregate<BsonDocument>(pipeline).ToListAsync();

            return results
                .Where(d => d["_id"].IsObjectId)   // skip tickets with a missing reportedBy
                .ToDictionary(d => d["_id"].AsObjectId, d => d["count"].AsInt32);
        }
    }
}
