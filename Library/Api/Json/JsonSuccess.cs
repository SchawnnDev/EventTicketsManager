using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Library.Api.Json
{
    public class JsonSuccess
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        public JsonSuccess(bool success)
        {
            Success = success;
        }

    }
}
