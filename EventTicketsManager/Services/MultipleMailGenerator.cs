using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DinkToPdf.Contracts;
using Library;
using Library.Pdf;
using SendGrid;
using SendGrid.Helpers.Mail;
using Server;

namespace EventTicketsManager.Services
{
    public class MultipleMailGenerator
    {
        private readonly SaveableEvent _saveableEvent;
        private readonly List<MultipleMail> _multipleMails;
        private readonly IConverter _converter;
        private readonly SendGridClient _client;
        private readonly ServerContext _serverContext;
        private readonly string _userId;


        public MultipleMailGenerator(SaveableEvent saveableEvent, List<MultipleMail> multipleMails, IConverter converter, ServerContext serverContext, string apiKey, string userId)
        {
            _saveableEvent = saveableEvent;
            _multipleMails = multipleMails;
            _converter = converter;
            _serverContext = serverContext;
            _client = new SendGridClient(apiKey);
            _userId = userId;
        }

        public async Task SendAllMailsAsync()
        {
            var htmlContent = GetHtmlContent();
            string pdfHtmlContent = null;
            var from = new EmailAddress(_saveableEvent.Email, _saveableEvent.Name);
            

            foreach (var multipleMail in _multipleMails)
            {
                var subject = $"Billet n°0{multipleMail.Ticket.Id} - {_saveableEvent.Name}";
                var to = new EmailAddress(multipleMail.Ticket.Email);
                var msg = MailHelper.CreateSingleEmail(from, to, subject, "", ConvertHtmlContent(htmlContent, multipleMail.Ticket));
                var pdf = new PdfGenerator(multipleMail.QrCode, _converter);
                if (pdfHtmlContent != null)
                    pdf.HtmlContent = pdfHtmlContent;

                var pdfBytes = pdf.Generate();

                if (pdfHtmlContent == null)
                    pdfHtmlContent = pdf.HtmlContent;

                await using (var stream = new MemoryStream(pdfBytes))
                {
                    await msg.AddAttachmentAsync($"Billet0{multipleMail.Ticket.Id}_{multipleMail.Ticket.FirstName}{multipleMail.Ticket.LastName}.pdf", stream);
                }

                await _client.SendEmailAsync(msg);

                _serverContext.TicketUserMails.Add(new SaveableTicketUserMail(multipleMail.Ticket, _userId, DateTime.UtcNow));

                Logger.SendLog($"Sent mail for ticket n°0{multipleMail.Ticket.Id}", _userId, _serverContext);
            }

        }

        private string GetHtmlContent()
        {
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Models/Html/MailModel.html");
            var content = File.ReadAllText(path);
            return content;
        }

        private string ConvertHtmlContent(string htmlContent, SaveableTicket ticket)
        {
	        var specificCulture = CultureInfo.CreateSpecificCulture("fr-FR");
			var toPay = ticket.ToPay.ToString("C", specificCulture);
            return htmlContent.Replace("{event.name}", _saveableEvent.Name)
                .Replace("{event.email}", _saveableEvent.Email)
                .Replace("{event.postalCode}", _saveableEvent.PostalCode)
                .Replace("{event.city}", _saveableEvent.CityName)
                .Replace("{event.address}", string.IsNullOrEmpty(_saveableEvent.AddressNumber) || _saveableEvent.AddressNumber.Equals("0") ? _saveableEvent.AddressName : $"{_saveableEvent.AddressNumber} {_saveableEvent.AddressName}")
                .Replace("{event.postalCode}", _saveableEvent.PostalCode)
                .Replace("{event.start}", $"{_saveableEvent.Start.ToString("dd/MM/YYYY à HH:mm", specificCulture)}".Replace(":", "h").Replace("YYYY", Math.Abs(_saveableEvent.Start.Year - 2000).ToString()))
                .Replace("{event.end}", $"{_saveableEvent.End.ToString("dd/MM/YYYY à HH:mm", specificCulture)}".Replace(":", "h").Replace("YYYY", Math.Abs(_saveableEvent.End.Year - 2000).ToString()))
                .Replace("{event.headerUrl}", _saveableEvent.HeaderUrl)
                .Replace("{ticket.id}", ticket.Id.ToString())
                .Replace("{ticket.firstName}", ticket.FirstName)
                .Replace("{ticket.lastName}", ticket.LastName)
                .Replace("{ticket.emailContent}", _saveableEvent.EmailContent)
                .Replace("{ticket.toPay}", $"{toPay}")
                .Replace("{ticket.name}", "Billet d'entrée")
                .Replace("{ticket.quantity}", "1")
                .Replace("{ticket.subTotal}", $"{toPay}")
                .Replace("{ticket.totalPrice}", $"{toPay}");
        }

    }

    public struct MultipleMail
    {
        public SaveableTicket Ticket { get; set; }
        public SaveableTicketQrCode QrCode { get; set; }
    }
}
