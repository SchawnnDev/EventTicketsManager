﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Server
{
    [Table("TicketScans")]
	public class SaveableTicketScan
	{

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public SaveableTicketScan() { }

		public SaveableTicketScan(SaveableTicket ticket, string creatorId, DateTime date)
		{
			Ticket = ticket;
			CreatorId = creatorId;
			Date = date;
		}

		public SaveableTicket Ticket { get; set; }
        public string CreatorId { get; set; }

        public DateTime Date { get; set; }

	}
}
