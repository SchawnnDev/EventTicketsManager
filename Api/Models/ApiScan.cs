using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Models
{
	public class ApiScan
	{

		public ApiKeyLogin Login { get; set; }

		public string QrCode { get; set; }

		public ApiScan(string apiKey, string email, string qrCode) : this(new ApiKeyLogin(apiKey, email), qrCode)
		{
		}

		public ApiScan(ApiKeyLogin login, string qrCode)
		{
			Login = login;
			QrCode = qrCode;
		}
	}
}
