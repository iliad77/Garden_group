// Author: Luca Mulders
using Garden_group.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Garden_group.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ITicketRepository _ticketRepository;

        public TicketsController(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }

        // Lists all tickets with the status counts for the whole service desk
        public async Task<IActionResult> Index()
        {
            ViewBag.StatusCounts = await _ticketRepository.GetStatusCountsAsync();
            var tickets = await _ticketRepository.GetAllAsync();
            return View(tickets);
        }
    }
}
