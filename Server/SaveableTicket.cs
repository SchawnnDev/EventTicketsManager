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

		public SaveableTicket(string firstName, string lastName, string email,
			DateTime lastConnection, DateTime creationDate)
		{
			FirstName = firstName;
			LastName = lastName;
			Email = email;
			LastConnection = lastConnection;
			CreationDate = creationDate;
		}

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public string FirstName { get; set; }

		public string LastName { get; set; }

		public string Email { get; set; }

		public decimal ToPay { get; set; } 

		public bool HasPaid { get; set; }

		public bool Sex { get; set; }

		public DateTime LastConnection { get; set; }

		public DateTime CreationDate { get; set; }

		public SaveableEvent Event { get; set; }

        public IdentityUser Creator { get; set; }

	}
}
