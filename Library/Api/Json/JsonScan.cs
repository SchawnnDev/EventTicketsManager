using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Library.Api.Json
{
    public class JsonScan
    {

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("hasPaid")]
        public bool HasPaid { get; set; }

        [JsonProperty("alreadyScanned")]
        public bool AlreadyScanned { get; set; }

        [JsonProperty("toPay")]
        public decimal ToPay { get; set; }

		[JsonProperty("success")]
		public bool Success { get; set; }

		[JsonProperty("error")]
		public string Error { get; set; }

		[JsonProperty("lastScanDate")]
		public string LastScanDate { get; set; }

		public JsonScan(bool success, string error)
		{
			Success = success;
			Error = error;
		}

        public JsonScan(string firstName, string lastName, bool hasPaid, bool alreadyScanned, decimal toPay, DateTime lastScanDate)
        {
            FirstName = firstName;
            LastName = lastName;
            HasPaid = hasPaid;
            AlreadyScanned = alreadyScanned;
            ToPay = toPay;
            Success = true;
            LastScanDate = alreadyScanned ? lastScanDate.ToString("dd/MM/YYYY à HH:mm", CultureInfo.CreateSpecificCulture("fr-FR"))
	            .Replace(":", "h").Replace("YYYY", Math.Abs(lastScanDate.Year - 2000).ToString()) : "0";
        }

    }
}
