// Author: Luca Mulders
using Garden_group.Models;

namespace Garden_group.Repositories
{
    /// <summary>
    /// Data access for the tickets collection.
    /// </summary>
    public interface ITicketRepository
    {
        /// <summary>
        /// Gets every ticket, newest first.
        /// </summary>
        /// <returns>All tickets, or an empty list if there are none.</returns>
        Task<List<Ticket>> GetAllAsync();

        /// <summary>
        /// Gets the tickets reported by one user, newest first.
        /// </summary>
        /// <param name="userId">The ObjectId of the reporting user, as a string.</param>
        /// <returns>The user's tickets, or an empty list if there are none or the id is invalid.</returns>
        Task<List<Ticket>> GetByUserAsync(string userId);

        /// <summary>
        /// Gets a single ticket.
        /// </summary>
        /// <param name="id">The ObjectId of the ticket, as a string.</param>
        /// <returns>The ticket, or null if it doesn't exist or the id is invalid.</returns>
        Task<Ticket?> GetByIdAsync(string id);

        /// <summary>
        /// Inserts a new ticket. The status is always set to Open and handledBy starts empty.
        /// </summary>
        /// <param name="ticket">The ticket to insert. Any Id it already has is ignored.</param>
        /// <returns>The inserted ticket with its generated Id.</returns>
        Task<Ticket> CreateAsync(Ticket ticket);

        /// <summary>
        /// Replaces all fields of an existing ticket, including its status.
        /// </summary>
        /// <param name="ticket">The ticket with its new values. Its Id decides which ticket is replaced.</param>
        /// <returns>True if the ticket was found, false otherwise.</returns>
        Task<bool> UpdateAsync(Ticket ticket);

        /// <summary>
        /// Sets a new status and adds the action to handledBy in one query.
        /// This is how service desk employees close tickets, by passing TicketStatus.Closed or TicketStatus.Resolved.
        /// </summary>
        /// <param name="id">The ObjectId of the ticket, as a string.</param>
        /// <param name="status">The new status.</param>
        /// <param name="handling">Who made the change, when, and a comment.</param>
        /// <returns>True if the ticket was found, false otherwise.</returns>
        Task<bool> ChangeStatusAsync(string id, TicketStatus status, TicketHandling handling);

        /// <summary>
        /// Permanently removes a ticket, for example a duplicate or a ticket created by mistake.
        /// This is not how tickets are closed. Use <see cref="ChangeStatusAsync"/> for that, so the history is kept.
        /// </summary>
        /// <param name="id">The ObjectId of the ticket, as a string.</param>
        /// <returns>True if a ticket was deleted, false if it wasn't found.</returns>
        Task<bool> DeleteAsync(string id);

        /// <summary>
        /// Counts tickets per status for the dashboard, using an aggregation pipeline.
        /// </summary>
        /// <param name="userId">The ObjectId of a user to count only their tickets, or null to count all tickets.</param>
        /// <returns>The number of tickets for every status. Statuses without tickets are included as 0.</returns>
        Task<Dictionary<TicketStatus, int>> GetStatusCountsAsync(string? userId = null);
    }
}
