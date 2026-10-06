using Garden_group.Models;

namespace Garden_group.Repositories
{
    public interface ITicketRepository
    {
        Task<List<Ticket>> GetAllAsync();
        Task<List<Ticket>> GetByUserAsync(string userId);
        Task<Ticket?> GetByIdAsync(string id);
        Task<Ticket> CreateAsync(Ticket ticket);
        Task<bool> UpdateAsync(Ticket ticket);
        Task<bool> ChangeStatusAsync(string id, TicketStatus status, TicketHandling handling);
        Task<bool> DeleteAsync(string id);
        Task<Dictionary<TicketStatus, int>> GetStatusCountsAsync(string? userId = null);
    }
}
