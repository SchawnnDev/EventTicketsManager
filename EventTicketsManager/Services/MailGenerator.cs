using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DinkToPdf.Contracts;
using EventTicketsManager.Services;
using Library.Pdf;
using Library.Threads;
using SendGrid;
using SendGrid.Helpers.Mail;
using Server;

namespace EventTicketsManager.Services
{
	public class MailGenerator
	{

		private SaveableTicket Ticket { get;}
        private readonly IConverter _converter;

		public MailGenerator(SaveableTicket ticket, IConverter converter)
		{
			Ticket = ticket;
            _converter = converter;
        }

		public async Task SendMailAsync(string apiKey, SaveableTicketQrCode qrCode)
		{
			var client = new SendGridClient(apiKey);
			var from = new EmailAddress(Ticket.Event.Email, Ticket.Event.Name);
			var subject = $"Billet - {Ticket.Event.Name}";
			var to = new EmailAddress(Ticket.Email);
			var msg = MailHelper.CreateSingleEmail(from, to, subject, "", GetHtmlContent());

			var pdf = new PdfGenerator(qrCode, _converter);
			var pdfBytes = pdf.Generate();

			await using (var stream = new MemoryStream(pdfBytes))
			{
				await msg.AddAttachmentAsync($"Billet0{Ticket.Id}_{Ticket.FirstName}{Ticket.LastName}.pdf", stream);
				await stream.DisposeAsync();
			}

			await client.SendEmailAsync(msg);
		}

		private string GetHtmlContent()
		{
			var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"Models/Html/MailModel.html");
			var content = File.ReadAllText(path);
			var specificCulture = CultureInfo.CreateSpecificCulture("fr-FR");
			var toPay = Ticket.ToPay.ToString("C", specificCulture);
			content = content.Replace("{event.name}", Ticket.Event.Name)
				.Replace("{event.email}", Ticket.Event.Email)
				.Replace("{event.postalCode}", Ticket.Event.PostalCode)
				.Replace("{event.city}", Ticket.Event.CityName)
				.Replace("{event.address}", string.IsNullOrEmpty(Ticket.Event.AddressNumber) || Ticket.Event.AddressNumber.Equals("0") ? Ticket.Event.AddressName:$"{Ticket.Event.AddressNumber} {Ticket.Event.AddressName}")
				.Replace("{event.postalCode}", Ticket.Event.PostalCode)
				.Replace("{event.start}", $"{Ticket.Event.Start.ToString("dd/MM/YYYY à HH:mm", specificCulture)}".Replace(":","h").Replace("YYYY", Math.Abs(Ticket.Event.Start.Year - 2000).ToString()))
				.Replace("{event.end}", $"{Ticket.Event.End.ToString("dd/MM/YYYY à HH:mm", specificCulture)}".Replace(":", "h").Replace("YYYY", Math.Abs(Ticket.Event.End.Year - 2000).ToString()))
				.Replace("{event.headerUrl}", Ticket.Event.HeaderUrl)
				.Replace("{ticket.id}", Ticket.Id.ToString())
				.Replace("{ticket.firstName}", Ticket.FirstName)
				.Replace("{ticket.lastName}", Ticket.LastName)
				.Replace("{ticket.emailContent}", Ticket.Event.EmailContent)
				.Replace("{ticket.toPay}", $"{toPay}")
				.Replace("{ticket.name}", "Billet d'entrée")
				.Replace("{ticket.quantity}","1")
				.Replace("{ticket.subTotal}", $"{toPay}")
				.Replace("{ticket.totalPrice}", $"{toPay}");
			return content;
		}

	}
}
