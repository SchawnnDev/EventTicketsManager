using System;
using System.Collections.Generic;
using System.Text;
using Server;

namespace Library
{
	public class Logger
	{

		public static void SendLog(string message, string user, ServerContext context)
		{
			context.Logs.Add(new SaveableLog {Date = DateTime.Now, Message = message, UserId = user});
		}

	}
}
