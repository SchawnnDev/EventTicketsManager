using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Library.Utils
{
	public class MailUtils
	{

		private const string StrRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
		@"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
		@".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";

		public static bool IsEmailValid(string inputEmail)
		{
			if (string.IsNullOrWhiteSpace(inputEmail)) return false;
			var re = new Regex(StrRegex);
			return re.IsMatch(inputEmail);
		}
	}
}
