using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Server;

namespace EventTicketsManager.Models
{
    public class TicketIndexModel
    {

        public List<SaveableEvent> UserEvents { get; set; }

        public string Error { get; set; }

        public TicketIndexModel(List<SaveableEvent> userEvents) : this(userEvents, null) { }

        public TicketIndexModel(List<SaveableEvent> userEvents, string error)
        {
            UserEvents = userEvents;
            Error = error;
        }

        public bool IsError() => !string.IsNullOrEmpty(Error);

    }
}
