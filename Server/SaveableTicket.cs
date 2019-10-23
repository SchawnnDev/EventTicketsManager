using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace Server
{
	[Table("Tickets")]
	public class SaveableTicket
	{
		public SaveableTicket()
		{
		}

		public SaveableTicket(string firstName, string lastName, string email)
		{
			FirstName = firstName;
			LastName = lastName;
			Email = email;
        }

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public string FirstName { get; set; }

		public string LastName { get; set; }

		public string Email { get; set; }

		public decimal ToPay { get; set; } 

		public bool HasPaid { get; set; }

		public int Gender { get; set; }

        public SaveableEvent Event { get; set; }

        public string CreatorId { get; set; }

        public string UpdaterId { get; set; }

        public DateTime UpdatedAt { get; set; }

        public DateTime CreatedAt { get; set; }

        [NotMapped] public int TicketEventId { get; set; }

    }
}
