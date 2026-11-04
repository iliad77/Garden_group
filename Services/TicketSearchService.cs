// Author: Chris
using Garden_group.Models;
using Garden_group.Repositories;

namespace Garden_group.Services
{
    public class TicketSearchService
    {
        private readonly ITicketRepository _ticketRepository;

        public TicketSearchService(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }

        /// <summary>
        /// Executes full database search with AND/OR logic and ordering.
        /// </summary>
        public async Task<List<Ticket>> SearchTicketsAsync(string query, bool useAnd)
        {
            return await _ticketRepository.SearchTicketsAsync(query, useAnd);
        }
    }
}