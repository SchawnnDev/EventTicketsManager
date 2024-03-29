﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Server
{
	[Table("QrCodes")]
	public class SaveableTicketQrCode
	{

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public SaveableTicket Ticket { get; set; }

		public byte[] Key { get; set; }

		public byte[] IV { get; set; }

		public string CreatorId { get; set; }

		public DateTime CreatedAt { get; set; }
	}
}
