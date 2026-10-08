// Author: Luca Mulders
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
            if (!IsValidId(userId))
                return new List<Ticket>();

            return await _tickets.Find(t => t.ReportedBy == userId)
                .SortByDescending(t => t.TimeReported)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<Ticket?> GetByIdAsync(string id)
        {
            if (!IsValidId(id))
                return null;

            return await _tickets.Find(t => t.Id == id).FirstOrDefaultAsync();
        }

        /// <inheritdoc/>
        public async Task<Ticket> CreateAsync(Ticket ticket)
        {
            ticket.Id = null;
            ticket.Status = TicketStatus.Open;
            ticket.HandledBy = new List<TicketHandling>();

            await _tickets.InsertOneAsync(ticket);
            return ticket;
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateAsync(Ticket ticket)
        {
            if (!IsValidId(ticket.Id))
                return false;

            var result = await _tickets.ReplaceOneAsync(t => t.Id == ticket.Id, ticket);
            return result.MatchedCount > 0;
        }

        /// <inheritdoc/>
        public async Task<bool> ChangeStatusAsync(string id, TicketStatus status, TicketHandling handling)
        {
            if (!IsValidId(id))
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
            if (!IsValidId(id))
                return false;

            var result = await _tickets.DeleteOneAsync(t => t.Id == id);
            return result.DeletedCount > 0;
        }

        /// <inheritdoc/>
        public async Task<Dictionary<TicketStatus, int>> GetStatusCountsAsync(string? userId = null)
        {
            // Every status starts at 0 so the dashboard also shows statuses with no tickets
            var counts = new Dictionary<TicketStatus, int>
            {
                { TicketStatus.Open, 0 },
                { TicketStatus.Resolved, 0 },
                { TicketStatus.Closed, 0 }
            };

            if (userId != null && !IsValidId(userId))
                return counts;

            // No userId means all tickets, otherwise only the tickets this user reported
            FilterDefinition<Ticket> filter;
            if (userId == null)
                filter = FilterDefinition<Ticket>.Empty;
            else
                filter = Builders<Ticket>.Filter.Eq(t => t.ReportedBy, userId);

            // Aggregation pipeline: $match on the user (optional), then $group by status and count
            var groups = await _tickets.Aggregate()
                .Match(filter)
                .Group(t => t.Status, g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var group in groups)
                counts[group.Status] = group.Count;

            return counts;
        }

        // Checks that the id is a valid ObjectId (24 hex characters)
        // An invalid id would make the driver throw, so it's treated as not found instead
        private static bool IsValidId(string? id)
        {
            return ObjectId.TryParse(id, out _);
        }
    }
}
