using System;
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

		public SaveableTicket Ticket { get; set; }

		public DateTime Date { get; set; }

	}
}
