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

        // Returns every ticket, newest first
        public async Task<List<Ticket>> GetAllAsync()
        {
            return await _tickets.Find(FilterDefinition<Ticket>.Empty)
                .SortByDescending(t => t.TimeReported)
                .ToListAsync();
        }

        // Returns the tickets reported by userId, newest first (empty list if the id is invalid)
        public async Task<List<Ticket>> GetByUserAsync(string userId)
        {
            if (!ObjectId.TryParse(userId, out _))
                return new List<Ticket>();

            return await _tickets.Find(t => t.ReportedBy == userId)
                .SortByDescending(t => t.TimeReported)
                .ToListAsync();
        }

        // Returns the ticket with this id, or null if it doesn't exist
        public async Task<Ticket?> GetByIdAsync(string id)
        {
            // An invalid id string would make the driver throw, so treat it as not found
            if (!ObjectId.TryParse(id, out _))
                return null;

            return await _tickets.Find(t => t.Id == id).FirstOrDefaultAsync();
        }

        // Inserts the ticket as Open and returns it with its generated id
        public async Task<Ticket> CreateAsync(Ticket ticket)
        {
            ticket.Id = null;
            ticket.Status = TicketStatus.Open;
            ticket.HandledBy = new List<TicketHandling>();

            await _tickets.InsertOneAsync(ticket);
            return ticket;
        }

        // Replaces all fields of an existing ticket, returns false if it wasn't found
        public async Task<bool> UpdateAsync(Ticket ticket)
        {
            if (!ObjectId.TryParse(ticket.Id, out _))
                return false;

            var result = await _tickets.ReplaceOneAsync(t => t.Id == ticket.Id, ticket);
            return result.MatchedCount > 0;
        }

        // Sets a new status and adds the handling entry to handledBy, returns false if the ticket wasn't found
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

        // Permanently deletes the ticket, returns false if it wasn't found
        public async Task<bool> DeleteAsync(string id)
        {
            if (!ObjectId.TryParse(id, out _))
                return false;

            var result = await _tickets.DeleteOneAsync(t => t.Id == id);
            return result.DeletedCount > 0;
        }

        // Returns the number of tickets per status, for one user or for everyone if userId is null
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
    }
}
