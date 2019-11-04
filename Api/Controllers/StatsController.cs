using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Models;
using Library.Api.Json;
using Library.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server;

namespace Api.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class StatsController : ControllerBase
	{

		private readonly ServerContext _context;

		public StatsController(ServerContext context)
		{
			_context = context;
		}

		[HttpGet]
		public StatsModel Get(string id)
		{

			if (string.IsNullOrWhiteSpace(id))
				return new StatsModel { Success = false };

			ApiKeyLogin login;

			try
			{
				login = ParseLogin(id);
			}
			catch (Exception)
			{
				return new StatsModel { Success = false };
			}

			if(!LoginController.CheckLogin(login, _context))
				return new StatsModel { Success = false };

			var eventId = _context.Events.Where(t => t.ApiKey.Equals(login.ApiKey)).Select(t=>t.Id).Single();
			var model = new StatsModel
			{
				TicketsCount = _context.Tickets.Count(t => t.Event.Id == eventId),
				ScannedTicketsCount = _context.Tickets.Count(t =>
					t.Event.Id == eventId && _context.TicketScans.Any(e => e.Ticket.Id == t.Id)),
				PayedTicketsCount = _context.Tickets.Count(t => t.Event.Id == eventId && t.HasPaid),
				ScannedPayedTicketsCount = _context.Tickets.Count(t =>
					t.Event.Id == eventId && t.HasPaid && _context.TicketScans.Any(e => e.Ticket.Id == t.Id)),
				Success = true
			};
			return model;
		}

		private ApiKeyLogin ParseLogin(string text)
		{
			var decodedText = text.Base64Decode();
			var split = decodedText.Split("§§");
			return new ApiKeyLogin(split[0], split[1]);
		}

	}
}