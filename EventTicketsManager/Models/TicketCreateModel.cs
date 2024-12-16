using Server;

namespace EventTicketsManager.Models;

public class TicketCreateModel
{
    public TicketCreateModel(SaveableTicket ticket) : this(ticket, null)
    {
    }

    public TicketCreateModel(SaveableTicket ticket, string error)
    {
        Ticket = ticket;
        Error = error;
    }

    public SaveableTicket Ticket { get; set; }

    public string Error { get; set; }

    public bool IsError()
    {
        return !string.IsNullOrEmpty(Error);
    }
}