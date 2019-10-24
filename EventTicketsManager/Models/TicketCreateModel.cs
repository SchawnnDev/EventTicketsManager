using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Server;

namespace EventTicketsManager.Models
{
    public class TicketCreateModel
    {

        public SaveableTicket Ticket { get; set; }

        public string Error { get; set; }

        public TicketCreateModel(SaveableTicket ticket) : this(ticket, null) { }

        public TicketCreateModel(SaveableTicket ticket, string error)
        {
            Ticket = ticket;
            Error = error;
        }

        public bool IsError() => !string.IsNullOrWhiteSpace(Error);

    }
}
