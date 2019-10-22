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

        public TicketIndexModel(List<SaveableEvent> userEvents)
        {
            UserEvents = userEvents;
        }

    }
}
