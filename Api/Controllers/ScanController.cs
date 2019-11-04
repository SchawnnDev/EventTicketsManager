using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Models;
using Library.Api;
using Library.Api.Json;
using Library.Utils;
using Microsoft.AspNetCore.Mvc;
using Server;

namespace Api.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class ScanController : ControllerBase
	{
	    private readonly ServerContext _context;

	    public ScanController(ServerContext context)
	    {
		    _context = context;
	    }

	    [HttpGet]
		public JsonScan Get(string id)
		{
			if (string.IsNullOrWhiteSpace(id))
				return new JsonScan(false, "Input is null");

			ApiScan scan;

			try
			{
				scan = ParseScan(id);
			}
			catch (Exception e)
			{
				return new JsonScan(false, e.Message);
			}

			return CheckScan(scan);
		}

		private JsonScan CheckScan(ApiScan scan)
		{
			if (!LoginController.CheckLogin(scan.Login, _context))
				return new JsonScan(false, "Login refused");

			try
			{
				var firstDecode = scan.QrCode.Base64Decode();
				var split = firstDecode.Split("§§");

				var qrCodeId = int.Parse(split[0]);
				var qrCode = split[1];

				if (!_context.QrCodes.Any(t => t.Id == qrCodeId))
					return new JsonScan(false, "Qr Code not existing");

				var saveableQrCode = _context.QrCodes.Single(t => t.Id == qrCodeId);

				var qrCodeGenerator = new QrCodeGenerator(saveableQrCode);

				var decodedString = qrCodeGenerator.Decode(qrCode);

				var decodedSplit = decodedString.Split("§§");

				var ticketId = int.Parse(decodedSplit[0]);
				var ticketEmail = decodedSplit[1];

				if (!_context.Tickets.Any(t => t.Id == ticketId && t.Email.Equals(ticketEmail)))
					return new JsonScan(false, "Ticket id does not match with email");

				var ticket = _context.Tickets.Single(t => t.Id == ticketId && t.Email.Equals(ticketEmail));

				var alreadyScanned = _context.TicketScans.Any(t => t.Ticket.Id == ticket.Id);

				var lastScan = DateTime.Now;
				
				if(alreadyScanned)
					lastScan = _context.TicketScans.Where(t=>t.Ticket.Id == ticket.Id).OrderByDescending(t => t.Date).Select(t=>t.Date)
						.FirstOrDefault();

				var user = _context.Users.Any(t => t.Email.Equals(scan.Login.Email)) ?_context.Users.Where(t => t.Email.Equals(scan.Login.Email)).Select(t => t.Id).Single() :scan
					.Login.Email;

				_context.TicketScans.Add(new SaveableTicketScan(ticket, user, DateTime.Now));
				_context.SaveChanges();

				return new JsonScan(ticket.FirstName, ticket.LastName, ticket.HasPaid, alreadyScanned, ticket.ToPay, lastScan);

			}
			catch (Exception e)
			{
				return new JsonScan(false, e.Message);
			}
		}

		private ApiScan ParseScan(string text)
		{
			var decodedText = text.Base64Decode();
			var split = decodedText.Split("§§");
			return new ApiScan(split[0], split[1], split[2]);
		}
	}

}