namespace Garden_group.Models
{
    // Stored in MongoDB as strings, so names must match the data exactly
    public enum TicketStatus
    {
        Open,       // not yet solved
        Resolved,   // closed and solved
        Closed      // closed without being solved
    }

    public enum TicketPriority
    {
        Low,
        Medium,
        High
    }

    public enum IncidentType
    {
        Hardware,
        Software,
        Service
    }
}
