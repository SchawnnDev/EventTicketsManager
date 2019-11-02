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
    public class ScanController : Controller
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
				return null;

			ApiScan scan;

			try
			{
				scan = ParseScan(id);
			}
			catch (Exception)
			{
				return null;
			}

			return CheckScan(scan);
		}

		private JsonScan CheckScan(ApiScan scan)
		{
			if (!LoginController.CheckLogin(scan.Login, _context))
				return null;

			try
			{
				var firstDecode = scan.QrCode.Base64Decode();
				var split = firstDecode.Split("§§");

				var qrCodeId = int.Parse(split[0]);
				var qrCode = split[1];

				if (!_context.QrCodes.Any(t => t.Id == qrCodeId))
					return null;

				var saveableQrCode = _context.QrCodes.Single(t => t.Id == qrCodeId);

				var qrCodeGenerator = new QrCodeGenerator(saveableQrCode);

				var decodedString = qrCodeGenerator.Decode(qrCode);

				var decodedSplit = decodedString.Split("§§");

				var ticketId = int.Parse(decodedSplit[0]);
				var ticketEmail = decodedSplit[1];

				if (!_context.Tickets.Any(t => t.Id == ticketId && t.Email.Equals(ticketEmail)))
					return null;

				var ticket = _context.Tickets.Single(t => t.Id == ticketId && t.Email.Equals(ticketEmail));

				var alreadyScanned = _context.TicketScans.Any(t => t.Ticket.Id == ticket.Id);

				var user = _context.Users.Where(t => t.Email.Equals(ticketEmail)).Select(t => t.Id).Single();

				_context.TicketScans.Add(new SaveableTicketScan(ticket, user, DateTime.Now));
				_context.SaveChanges();

				return new JsonScan(ticket.FirstName, ticket.LastName, ticket.HasPaid, alreadyScanned, ticket.ToPay);

			}
			catch (Exception e)
			{
				return null;
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