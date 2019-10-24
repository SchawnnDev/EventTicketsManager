using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Server;

namespace EventTicketsManager.Models
{
	public class TicketListModel
    {
        public SaveableEvent Event { get; set; }

		public List<SaveableTicket> Tickets { get; set; }

		public string Error { get; set; }

		public TicketListModel(SaveableEvent saveableEvent, List<SaveableTicket> tickets) : this(saveableEvent, tickets, null) { }

		public TicketListModel(SaveableEvent saveableEvent, List<SaveableTicket> tickets, string error)
		{
			Tickets = tickets;
			Error = error;
            Event = saveableEvent;
        }

		public bool IsError() => !string.IsNullOrEmpty(Error);

	}
}

