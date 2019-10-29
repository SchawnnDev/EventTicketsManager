using System;
using System.Collections.Generic;
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

        public JsonScan(string firstName, string lastName, bool hasPaid, bool alreadyScanned, decimal toPay)
        {
            FirstName = firstName;
            LastName = lastName;
            HasPaid = hasPaid;
            AlreadyScanned = alreadyScanned;
            ToPay = toPay;
        }

    }
}
