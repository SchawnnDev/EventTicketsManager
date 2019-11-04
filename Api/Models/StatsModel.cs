using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Api.Models
{
	public class StatsModel
	{
		[JsonProperty("success")]
		public bool Success { get; set; }

		[JsonProperty("tickets")]
		public int TicketsCount { get; set; }

		[JsonProperty("scannedTickets")]
		public int ScannedTicketsCount { get; set; }

		[JsonProperty("payedTickets")]
		public int PayedTicketsCount { get; set; }

		[JsonProperty("scannedPayedTickets")]
		public int ScannedPayedTicketsCount { get; set; }
	}
}
