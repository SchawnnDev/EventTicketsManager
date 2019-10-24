using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Server;

namespace EventTicketsManager.Models
{
    public class TicketDetailsModel
    {

        public SaveableTicket Ticket { get; set; }

        public bool TicketScanned { get; set; }

        public int TicketScanNumber { get; set; }

        public string CreatorEmail { get; set; }

        public TicketDetailsModel(SaveableTicket ticket, int ticketScanNumber, string creatorEmail)
        {
            Ticket = ticket;
            TicketScanNumber = ticketScanNumber;
            TicketScanned = ticketScanNumber != 0;
            CreatorEmail = creatorEmail;
        }

    }
}
