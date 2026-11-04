// Author: Chris
using Garden_group.Repositories;
using Garden_group.Services;
using Microsoft.AspNetCore.Mvc;

namespace Garden_group.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ITicketRepository _ticketRepository;
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

        // Functionality 1 - Search Action (AND / OR + Most Recent) (Chris)
        [HttpGet]
        public async Task<IActionResult> Search(string query, string searchMode = "AND")
        {
            ViewBag.StatusCounts = await _ticketRepository.GetStatusCountsAsync();

            bool useAnd = (searchMode?.ToUpper() != "OR");
            var searchResults = await _searchService.SearchTicketsAsync(query, useAnd);

            ViewBag.SearchQuery = query;
            ViewBag.SearchMode = searchMode;

            // Set error notification if no matches exist
            if (!string.IsNullOrWhiteSpace(query) && (!searchResults.Any()))
            {
                ViewBag.ErrorMessage = $"No tickets found matching the search query: \"{query}\".";
            }

            return View("Index", searchResults);
        }

        // Functionality 2 - Filter Loaded List Action (Chris)
        [HttpGet]
        public async Task<IActionResult> FilterLoaded(string keyword)
        {
            ViewBag.StatusCounts = await _ticketRepository.GetStatusCountsAsync();

            var preloadedTickets = await _ticketRepository.GetAllAsync();
            var filteredTickets = _filterService.FilterLoadedTickets(preloadedTickets, keyword);

            ViewBag.FilterKeyword = keyword;

            // Set error notification if no matches exist
            if (!string.IsNullOrWhiteSpace(keyword) && (!filteredTickets.Any()))
            {
                ViewBag.ErrorMessage = $"No tickets found containing the keyword: \"{keyword}\".";
            }

            return View("Index", filteredTickets);
        }
    }
}