using Garden_group.Repositories;
using Microsoft.AspNetCore.Mvc;
using Garden_group.Services;

namespace Garden_group.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ITicketRepository _ticketRepository;

        // Injected TicketSearchService and TicketFilterService (Chris)
        //this allows for separation of concerns, keeping the controller focused on handling HTTP requests and delegating business logic to services
        // this also prevent multiple call to datbase from construsctor and allows for better testability and maintainability of the codebase
        private readonly TicketSearchService _searchService;
        private readonly TicketFilterService _filterService;

        public TicketsController(
            ITicketRepository ticketRepository,
            TicketSearchService searchService,
            TicketFilterService filterService)
        {
            _ticketRepository = ticketRepository;
            _searchService = searchService;
            _filterService = filterService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.StatusCounts = await _ticketRepository.GetStatusCountsAsync();
            var tickets = await _ticketRepository.GetAllAsync();
            return View(tickets);
        }

        // Functionality 1 - Search Action (AND / OR + Most Recent) (Chris)[cite: 2]
        [HttpGet]
        public async Task<IActionResult> Search(string query, string searchMode = "AND")
        {
            // Retain status counts for dashboard m
            ViewBag.StatusCounts = await _ticketRepository.GetStatusCountsAsync();

            // Evaluate boolean flag: true for AND search, false for OR search
            bool useAnd = (searchMode?.ToUpper() != "OR");

            // Execute search on subject and description, returning results sorted most recent first
            var searchResults = await _searchService.SearchTicketsAsync(query, useAnd);

            // Preserve search state for the view inputs
            ViewBag.SearchQuery = query;
            ViewBag.SearchMode = searchMode;

            return View("Index", searchResults);
        }

        // Functionality 2 - Filter Loaded List Action (Chris)
        [HttpGet]
        public async Task<IActionResult> FilterLoaded(string keyword)
        {
            // Retain status counts for dashboard 
            ViewBag.StatusCounts = await _ticketRepository.GetStatusCountsAsync();

            // Load all tickets first, then filter the preloaded list in memory
            var preloadedTickets = await _ticketRepository.GetAllAsync();
            var filteredTickets = _filterService.FilterLoadedTickets(preloadedTickets, keyword);

            // Preserve filter keyword for the view input
            ViewBag.FilterKeyword = keyword;

            return View("Index", filteredTickets);
        }
    }
}