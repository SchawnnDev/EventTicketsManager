using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Server
{
	[Table("Accounts")]
	public class SaveableAccount
	{
		public SaveableAccount()
		{
		}

		public SaveableAccount(string firstName, string lastName, string email, byte[] password, byte[] salt,
			DateTime lastConnection, DateTime creationDate)
		{
			FirstName = firstName;
			LastName = lastName;
			Email = email;
			Password = password;
			Salt = salt;
			LastConnection = lastConnection;
			CreationDate = creationDate;
		}

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public string FirstName { get; set; }

		public string LastName { get; set; }

		public string Email { get; set; }

		public byte[] Password { get; set; }

		public byte[] Salt { get; set; }

		public DateTime LastConnection { get; set; }

		public DateTime CreationDate { get; set; }

		[NotMapped] public bool IsAuthenticated { get; set; }
	}
}
