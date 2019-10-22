using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace Server
{
	[Table("Events")]
	public class SaveableEvent
	{

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public string Name { get; set; }

		public string AddressName { get; set; }

		public string AddressNumber { get; set; }

		public string CityName { get; set; }

		public string PostalCode { get; set; }

		public string TelephoneNumber { get; set; }

		public string Email { get; set; }

		public string LogoUrl { get; set; }

		public string HeaderUrl { get; set; }

		public string EmailContent { get; set; }

		public decimal EnterPrice { get; set; }

		public string ApiKey { get; set; }

		public bool Enabled { get; set; }

		public DateTime Start { get; set; }

		public DateTime End { get; set; }

        public string CreatorId { get; set; }

        public DateTime UpdatedAt { get; set; }

        public DateTime CreatedAt { get; set; }

        [NotMapped] public bool IsCreator { get; set; } = true;

    }
}