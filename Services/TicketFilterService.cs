
// NEW CODE BY CHRIS: Functionality 2 - Dedicated Filter Class      
using System;
using System.Collections.Generic;
using System.Linq;
using Garden_group.Models;

namespace Garden_group.Services
{
    public class TicketFilterService
    {
        public IEnumerable<Ticket> FilterLoadedTickets(IEnumerable<Ticket> preloadedTickets, string keyword)
        {
            if (preloadedTickets == null)
            {
                return Enumerable.Empty<Ticket>();
            }

            if (string.IsNullOrWhiteSpace(keyword))
            {
                return preloadedTickets;
            }

            var lowerKeyword = keyword.Trim().ToLowerInvariant();

            return preloadedTickets.Where(ticket =>
                (!string.IsNullOrEmpty(ticket.Subject) && ticket.Subject.ToLowerInvariant().Contains(lowerKeyword)) ||
                (!string.IsNullOrEmpty(ticket.Description) && ticket.Description.ToLowerInvariant().Contains(lowerKeyword)) ||
                ticket.IncidentType.ToString().ToLowerInvariant().Contains(lowerKeyword) ||
                ticket.Status.ToString().ToLowerInvariant().Contains(lowerKeyword) ||
                ticket.Priority.ToString().ToLowerInvariant().Contains(lowerKeyword)
            );
        }
    }
}