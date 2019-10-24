using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Server
{
    [Table("TicketUserMails")]
	public class SaveableTicketUserMail
	{

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public SaveableTicketUserMail() { }

		public SaveableTicketUserMail(SaveableTicket ticket, string creatorId, DateTime date)
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
