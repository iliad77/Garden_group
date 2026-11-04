Project NoSQL: Incident Management System
Search & Live Filter | Author: Chris
1. Project Background & My Role
For our NoSQL course project, our group built an incident management application for The Garden Group. The core idea is simple: employees can report IT issues as tickets, and the service desk resolves them. Beyond the basic group CRUD requirements, the course rubric requires each student to build and document an individual functionality in a separate class to demonstrate independent problem-solving.
I took on the Search and Filtering requirements from the pre-approved topic list. When a service desk gets busy, having dozens or hundreds of tickets on screen becomes unusable. My goal was to make finding tickets intuitive, fast, and error-tolerant, whether a user wants to run a deep database search across all records or narrow down what's already on their screen.
Assignment Guideline Note: The project guide makes a clear technical distinction between searching and filtering. Searching queries the database directly using AND/OR logic and sorts newest first. Filtering works on an already loaded list in memory using common keywords without making another trip to MongoDB.

2. How I Structured the Code (Separation of Concerns)
To keep our team's codebase clean and easy to test, I followed a standard 3-tier MVC pattern. I made sure my logic wasn't dumped directly into the controller or crammed into Luca's repository methods. Instead, I separated concerns across three places:
•	TicketsController: Handles HTTP requests, keeps track of user inputs (like search text and radio button choices in ViewBag), recalculates dashboard counts, and checks if results came back empty.
•	TicketSearchService & TicketFilterService: My two dedicated service classes. They satisfy the rubric rule requiring individual functionality in separate classes and keep business logic out of the web layer.
•	TicketRepository: Handles direct MongoDB communication, building strongly typed filters with the official C# driver and sorting results efficiently.

3. Feature 1: Database Search (AND / OR + Most Recent)
For the database search, users can type multiple words into a search bar and choose whether they want an AND search or an OR search. Here is how I implemented it step-by-step:
3.1 Breaking down the query & handling casing
If someone searches for 'printer offline', the first thing the code does is split the string into individual words using query.Split(' '). I wrapped each word in a BsonRegularExpression(word, 'i'). The 'i' flag makes the query case-insensitive, so searching for 'PRINTER', 'Printer', or 'printer' yields the exact same matches. Each word is checked against both the Subject and Description fields using builder.Or.
3.2 Dynamic AND vs. OR logic
On the UI, I added radio buttons for AND and OR. In the repository, I check the useAnd boolean flag. If useAnd is true, I combine the word filters with builder.And(wordFilters). This means every word in the search box must appear somewhere in the ticket. If false, I combine them with builder.Or(wordFilters), which widens the search to tickets containing any of the terms.
3.3 Sorting newest tickets first
To satisfy the rubric requirement of showing recent items on top, I chained .SortByDescending(t => t.TimeReported) right onto the MongoDB Find query. Doing the sort on the database server is much faster than sorting in memory after fetching.
// Added to TicketRepository.cs (Author: Chris)
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
        wordFilters.Add(builder.Or(
            builder.Regex(t => t.Subject, regex),
            builder.Regex(t => t.Description, regex)
        ));
    }

    var combinedFilter = useAnd ? builder.And(wordFilters) : builder.Or(wordFilters);

    return await _tickets
        .Find(combinedFilter)
        .SortByDescending(t => t.TimeReported)
        .ToListAsync();
}

4. Feature 2: Filtering on the Preloaded List
While searching hits MongoDB directly, filtering works on data that is already sitting on the user's screen. I built this to give employees instant feedback without making them wait for a database round trip.
4.1 Backend in-memory filter (TicketFilterService)
In the controller, FilterLoaded calls GetAllAsync() once to grab the preloaded tickets, then passes that list into my TicketFilterService. The service uses a clean LINQ query (.Where) to check if the keyword matches the Subject, Description, or Status using StringComparison.OrdinalIgnoreCase. This satisfies the C# backend component requirement cleanly.
4.2 Instant client-side filtering (JavaScript)
On the frontend, I added an input box with onkeyup='filterPreloadedTickets()'. As the user types each letter, a short JavaScript function loops over the table rows (#ticketTableBody tr) and instantly hides any row whose text doesn't contain the keyword. This makes the table feel snappy and responsive.
// Added to TicketFilterService.cs (Author: Chris)
public class TicketFilterService
{
    public List<Ticket> FilterLoadedTickets(IEnumerable<Ticket> preloadedTickets, string? keyword)
    {
        if (preloadedTickets == null) return new List<Ticket>();
        if (string.IsNullOrWhiteSpace(keyword)) return preloadedTickets.ToList();

        var term = keyword.Trim();
        return preloadedTickets
            .Where(t =>
                (!string.IsNullOrEmpty(t.Subject) && t.Subject.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(t.Description) && t.Description.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                t.Status.ToString().Contains(term, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}

5. Design Decisions: Why I Did Not Use $lookup
During planning, our team discussed using MongoDB's $lookup to join tickets with the employees collection so users could filter by employee names. After looking closely at the rubric, I decided not to use $lookup for this feature for three reasons:
•	It contradicts the assignment definition: The rubric explicitly states that filtering operates on an 'already loaded list'. Using $lookup runs a brand-new aggregation pipeline on MongoDB, turning it into another database search instead of a true list filter.
•	Unnecessary DTO overhead: Joining employees into tickets requires creating extra DTO classes (like TicketWithReporterDto) and unwinding arrays, which adds complexity without providing any extra value for the search and filter tasks.
•	Aggregation is already covered: Our Deliverable 2 rubric requires an aggregation pipeline where appropriate. Luca already implemented this in GetStatusCountsAsync() using .Match() and .Group(). Using another aggregation here wasn't necessary.

6. Troubleshooting: Fixing the Startup Timeout Bug
While testing, we ran into a System.TimeoutException as soon as the app started up. When looking into it, we found this line inside the TicketRepository constructor:
_tickets.Indexes.CreateOne(new CreateIndexModel<Ticket>(...));

In ASP.NET Core, repository classes are registered as Scoped services, meaning the constructor runs on every single web request. Because constructors can't be asynchronous, this line was making a blocking network call to MongoDB Atlas before any page could load. If Atlas had any latency or our IP wasn't whitelisted yet, the entire application froze for 30 seconds and crashed. We fixed this by removing index creation from the constructor and moving it to an asynchronous startup task in Program.cs.
7.  Empty-State Feedback
A common issue in student projects is that typing a non-existent search term leaves a blank, confusing screen. I added clear empty-state feedback in two ways:
•	Server search notice: If a database search finds zero matching tickets, the controller sets a message in ViewBag.ErrorMessage. Index.cshtml then renders a clean Bootstrap alert banner saying no tickets matched, with a handy 'View all tickets' reset link.
•	Live filter notice: If the live JavaScript filter hides every row in the table, a hidden row (#noFilterMatchRow) immediately appears inside the table body informing the user that no loaded tickets match their keyword.

8. Rubric Alignment
Rubric Requirement	How My Implementation Meets It
Individual component in a separate class	Built TicketSearchService.cs and TicketFilterService.cs independently from teammate code.
Search functionality (AND/OR + newest first)	Tokenizes query, builds case-insensitive regex filters with dynamic AND/OR logic, and sorts by TimeReported descending directly in MongoDB.
Filter functionality on preloaded list	Filters an already-fetched list in memory using C# LINQ and provides live real-time DOM filtering via JavaScript.
Explaining architectural choices	Clearly documented why $lookup was rejected, why separate services were introduced, and how the constructor timeout was resolved.

