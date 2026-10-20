//  FUNCTIONALITY 1: TicketSearchService Class (Chris)                
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        
        // Searches tickets by matching words in Subject and Description/Content.
        //Supports AND and OR modes and sorts with the most recent tickets first.
        
        public async Task<List<Ticket>> SearchTicketsAsync(string searchQuery, bool useAndOperator = true)
        {
            // Retrieve all tickets using the verified working repository
            var allTickets = await _ticketRepository.GetAllAsync();

            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                return allTickets.OrderByDescending(t => t.TimeReported).ToList();
            }

            // Split into individual search terms and normalize
            var terms = searchQuery
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim().ToLowerInvariant())
                .ToList();

            if (!terms.Any())
            {
                return allTickets.OrderByDescending(t => t.TimeReported).ToList();
            }

            IEnumerable<Ticket> matches;

            if (useAndOperator)
            {
                // AND search: Every word must appear in either the Subject or Description[cite: 2]
                matches = allTickets.Where(ticket =>
                    terms.All(term =>
                        (!string.IsNullOrEmpty(ticket.Subject) && ticket.Subject.ToLowerInvariant().Contains(term)) ||
                        (!string.IsNullOrEmpty(ticket.Description) && ticket.Description.ToLowerInvariant().Contains(term))
                    )
                );
            }
            else
            {
                // OR search: At least one word must appear in either the Subject or Description[cite: 2]
                matches = allTickets.Where(ticket =>
                    terms.Any(term =>
                        (!string.IsNullOrEmpty(ticket.Subject) && ticket.Subject.ToLowerInvariant().Contains(term)) ||
                        (!string.IsNullOrEmpty(ticket.Description) && ticket.Description.ToLowerInvariant().Contains(term))
                    )
                );
            }

            // Always order by most recent on top as required by rubric[cite: 2]
            return matches.OrderByDescending(t => t.TimeReported).ToList();
        }
    }
}