// Author: Chris
using Garden_group.Models;

namespace Garden_group.Services
{
    public class TicketFilterService
    {
        /// <summary>
        /// Filters a preloaded list of tickets in memory using common keywords across subject, description, and status.
        /// </summary>
        public List<Ticket> FilterLoadedTickets(IEnumerable<Ticket> preloadedTickets, string? keyword)
        {
            if (preloadedTickets == null)
                return new List<Ticket>();

            if (string.IsNullOrWhiteSpace(keyword))
                return preloadedTickets.ToList();

            var term = keyword.Trim();

            return preloadedTickets
                .Where(t =>
                    (!string.IsNullOrEmpty(t.Subject) && t.Subject.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(t.Description) && t.Description.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    t.Status.ToString().Contains(term, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }
}