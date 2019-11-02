using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Models
{
	public class ApiKeyLogin
	{
		public string ApiKey { get; set; }
		public string Email { get; set; }

		public ApiKeyLogin(string apiKey, string email)
		{
			ApiKey = apiKey;
			Email = email;
		}
	}
}
