using System.Collections.Generic;
using Server;

namespace EventTicketsManager.Models;

public class TicketIndexModel
{
    public TicketIndexModel(List<SaveableEvent> userEvents) : this(userEvents, null)
    {
    }

    public TicketIndexModel(List<SaveableEvent> userEvents, string error)
    {
        UserEvents = userEvents;
        Error = error;
    }

    public List<SaveableEvent> UserEvents { get; set; }

    public string Error { get; set; }

    public bool IsError()
    {
        return !string.IsNullOrEmpty(Error);
    }
}