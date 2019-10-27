using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EventTicketsManager.Services;
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

		public async Task SendMailAsync(string apiKey, SaveableTicketQrCode qrCode)
		{
			var client = new SendGridClient(apiKey);
			var from = new EmailAddress(Ticket.Event.Email, Ticket.Event.Name);
			var subject = $"Billet - {Ticket.Event.Name}";
			var to = new EmailAddress(Ticket.Email);
			var msg = MailHelper.CreateSingleEmail(from, to, subject, "", GetHtmlContent());

			var pdf = new PdfGenerator(qrCode);

			await msg.AddAttachmentAsync($"{Ticket.FirstName}-{Ticket.LastName}-{Ticket.Event.Name}-Ticket.pdf", new MemoryStream(pdf.Generate()));

			await client.SendEmailAsync(msg);
		}

		private string GetHtmlContent()
		{
			var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Services\MailModel.html");
			var content = File.ReadAllText(path);
			var toPay = Ticket.ToPay.ToString("c");
			content = content.Replace("{event.name}", Ticket.Event.Name)
				.Replace("{event.email}", Ticket.Event.Email)
				.Replace("{event.postalCode}", Ticket.Event.PostalCode)
				.Replace("{event.city}", Ticket.Event.CityName)
				.Replace("{event.address}", string.IsNullOrEmpty(Ticket.Event.AddressNumber) ? Ticket.Event.AddressName:$"{Ticket.Event.AddressNumber} {Ticket.Event.AddressName}")
				.Replace("{event.postalCode}", Ticket.Event.PostalCode)
				.Replace("{event.start}", $"{Ticket.Event.Start:dd/MM/YYYY à HH:mm}".Replace(":","h").Replace("YYYY", Math.Abs(Ticket.Event.Start.Year - 2000).ToString()))
				.Replace("{event.end}", $"{Ticket.Event.End:dd/MM/YYYY à HH:mm}".Replace(":", "h").Replace("YYYY", Math.Abs(Ticket.Event.End.Year - 2000).ToString()))
				.Replace("{event.headerUrl}", Ticket.Event.HeaderUrl)
				.Replace("{ticket.id}", Ticket.Id.ToString())
				.Replace("{ticket.firstName}", Ticket.FirstName)
				.Replace("{ticket.lastName}", Ticket.LastName)
				.Replace("{ticket.emailContent}", Ticket.Event.EmailContent)
				.Replace("{ticket.toPay}", toPay)
				.Replace("{ticket.name}", "Billet d'entrée")
				.Replace("{ticket.quantity}","1")
				.Replace("{ticket.subTotal}", toPay)
				.Replace("{ticket.totalPrice}", toPay);
			return content;
		}

	}
}
