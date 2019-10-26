using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;
using Server;

namespace Library.Mail
{
	public class MailGenerator
	{

		private SaveableTicket Ticket { get;}

		public MailGenerator(SaveableTicket ticket)
		{
			Ticket = ticket;
		}

		public async Task SendMailAsync()
		{
			var apiKey = Environment.GetEnvironmentVariable("hej");
			var client = new SendGridClient(apiKey);
			var from = new EmailAddress(Ticket.Event.Email);
			var subject = $"Ticket - {Ticket.Event.Name}";
			var to = new EmailAddress(Ticket.Email);
			var body = "Hej!";
			var msg = MailHelper.CreateSingleEmail(from, to, subject, body, "");

			var pdf = new PdfGenerator();

			await msg.AddAttachmentAsync($"{Ticket.FirstName}-{Ticket.LastName}-{Ticket.Event.Name}-Ticket.pdf", new MemoryStream(pdf.Generate()));

			await client.SendEmailAsync(msg);
		}

	}
}
