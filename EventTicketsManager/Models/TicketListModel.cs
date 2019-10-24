using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Server;

namespace EventTicketsManager.Models
{
	public class TicketListModel
	{

		public List<SaveableTicket> Tickets { get; set; }

		public string Error { get; set; }

		public TicketListModel(List<SaveableTicket> tickets) : this(tickets, null) { }

		public TicketListModel(List<SaveableTicket> tickets, string error)
		{
			Tickets = tickets;
			Error = error;
		}

		public bool IsError() => !string.IsNullOrEmpty(Error);

	}
}

