using System.Collections.Generic;
using Server;

namespace EventTicketsManager.Models;

public class TicketListModel
{
    public TicketListModel(SaveableEvent saveableEvent, List<SaveableTicket> tickets) : this(saveableEvent, tickets,
        null)
    {
    }

    public TicketListModel(SaveableEvent saveableEvent, List<SaveableTicket> tickets, string error)
    {
        Tickets = tickets;
        Error = error;
        Event = saveableEvent;
        TicketMailCounts = new Dictionary<int, int>();
    }

    public SaveableEvent Event { get; set; }

    public List<SaveableTicket> Tickets { get; set; }

    public string Error { get; set; }

    public Dictionary<int, int> TicketMailCounts { get; set; }

    public bool IsError()
    {
        return !string.IsNullOrEmpty(Error);
    }
}