using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Models;
using Library.Api.Json;
using Library.Utils;
using Microsoft.AspNetCore.Mvc;
using Server;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Api.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class LoginController : ControllerBase
	{

		private readonly ServerContext _context;

		public LoginController(ServerContext context)
		{
			_context = context;
		}

		[HttpGet]
		public LoginModel Get(string id)
		{
			if (string.IsNullOrWhiteSpace(id))
				return new LoginModel {Success = false};

			ApiKeyLogin login;

			try
			{
				login = ParseLogin(id);
			}
			catch (Exception)
			{
				return new LoginModel {Success = false};
			}

			return new LoginModel {Success = CheckLogin(login, _context) };
		}

		public static bool CheckLogin(ApiKeyLogin login, ServerContext db)
		{
			if (!MailUtils.IsEmailValid(login.Email))
				return false;

			if (!db.Events.Any(t => t.ApiKey.Equals(login.ApiKey)) ||
			    !db.Users.Any(t => t.Email.ToLower().Equals(login.Email.ToLower())))
				return false;

			var eventId = db.Events.Where(t => t.ApiKey.Equals(login.ApiKey)).Select(t => t.Id).Single();
			var userId = db.Users.Where(t => t.Email.ToLower().Equals(login.Email.ToLower())).Select(t => t.Id)
				.Single();

			return DbUtils.IsEventExistingAndUserEventMember(eventId, userId, db);
		}

		private ApiKeyLogin ParseLogin(string text)
		{
			var decodedText = text.Base64Decode();
			var split = decodedText.Split("§§");
			return new ApiKeyLogin(split[0], split[1]);
		}

	}
}
