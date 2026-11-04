// Author: Luca Mulders & Chris
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
            var counts = Enum.GetValues<TicketStatus>().ToDictionary(s => s, s => 0);
            if (userId != null && !ObjectId.TryParse(userId, out _))
                return counts;

            var filter = userId == null
                ? FilterDefinition<Ticket>.Empty
                : Builders<Ticket>.Filter.Eq(t => t.ReportedBy, userId);

            var groups = await _tickets.Aggregate()
                .Match(filter)
                .Group(t => t.Status, g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var group in groups)
                counts[group.Status] = group.Count;

            return counts;
        }

        /// <inheritdoc/>
        // Author: Chris
        public async Task<List<Ticket>> SearchTicketsAsync(string query, bool useAnd)
        {
            if (string.IsNullOrWhiteSpace(query))
                return await GetAllAsync();

            var words = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var builder = Builders<Ticket>.Filter;
            var wordFilters = new List<FilterDefinition<Ticket>>();

            foreach (var word in words)
            {
                var regex = new BsonRegularExpression(word, "i");
                // Each word can appear in Subject OR Description
                wordFilters.Add(builder.Or(
                    builder.Regex(t => t.Subject, regex),
                    builder.Regex(t => t.Description, regex)
                ));
            }

            // Combine words using AND or OR based on toggle
            var combinedFilter = useAnd
                ? builder.And(wordFilters)
                : builder.Or(wordFilters);

            return await _tickets
                .Find(combinedFilter)
                .SortByDescending(t => t.TimeReported)
                .ToListAsync();
        }
    }
}