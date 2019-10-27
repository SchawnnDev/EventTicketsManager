using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using DinkToPdf;
using Library.Api;
using QRCoder;
using Server;

namespace Library.Pdf
{
	public class PdfGenerator
	{

		private readonly SynchronizedConverter Converter;
		private readonly SaveableTicketQrCode QrCode;

		public PdfGenerator(SaveableTicketQrCode qrCode)
		{
			Converter = new SynchronizedConverter(new PdfTools());
			QrCode = qrCode;
		}

		private string GenerateQrCode()
		{
			var generator = new QrCodeGenerator(QrCode);
			using var qrGenerator = new QRCodeGenerator();
			var qrCodeData = qrGenerator.CreateQrCode(generator.Get(), QRCodeGenerator.ECCLevel.M);
			var qrCode = new Base64QRCode(qrCodeData);
			return qrCode.GetGraphic(20);
		}

		private string GetHtmlContent()
		{
			var ticket = QrCode.Ticket;
			var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Services\PdfModel.html");
			var content = File.ReadAllText(path);
			var toPay = ticket.ToPay.ToString("c");
			content = content.Replace("{event.name}", ticket.Event.Name)
				.Replace("{event.email}", ticket.Event.Email)
				.Replace("{event.postalCodeCity}", $"{ticket.Event.PostalCode} {ticket.Event.CityName}")
				.Replace("{event.address}", string.IsNullOrEmpty(ticket.Event.AddressNumber) ? ticket.Event.AddressName : $"{ticket.Event.AddressNumber} {ticket.Event.AddressName}")
				.Replace("{event.postalCode}", ticket.Event.PostalCode)
				.Replace("{event.startDate}", $"{ticket.Event.Start:D}")
				.Replace("{event.startHour}", $"{ticket.Event.Start:HH:mm}".Replace(":","h"))
				.Replace("{ticket.date}", $"{ticket.CreatedAt:dd/MM/YYYY}".Replace("YYYY", Math.Abs(ticket.CreatedAt.Year - 2000).ToString()))
				.Replace("{event.headerUrl}", ticket.Event.HeaderUrl)
				.Replace("{ticket.id}", ticket.Id.ToString())
				.Replace("{ticket.firstName}", ticket.FirstName)
				.Replace("{ticket.lastName}", ticket.LastName)
				.Replace("{event.emailContent}", ticket.Event.EmailContent)
				.Replace("{ticket.toPay}", toPay)
				.Replace("{ticket.name}", "Billet d'entrée")
				.Replace("{ticket.quantity}", "1")
				.Replace("{ticket.hasToPay}", ticket.HasPaid ? "Vous avez déjà payé ce billet.":"Votre billet n'a pas encore été payé.")
				.Replace("{ticket.totalPrice}", toPay)
				.Replace("{ticket.qrcode}", GenerateQrCode());
			return content;
		}

		public byte[] Generate()
		{
			var doc = new HtmlToPdfDocument()
			{
				GlobalSettings = {
					ColorMode = ColorMode.Color,
					Orientation = Orientation.Portrait,
					PaperSize = PaperKind.A4Plus,
				},
				Objects = {
					new ObjectSettings() {
						PagesCount = true,
						HtmlContent = GetHtmlContent(),
						WebSettings = { DefaultEncoding = "utf-8" },
					//	HeaderSettings = { FontSize = 9, Right = "Page [page] of [toPage]", Line = true, Spacing = 2.812 }
					}
				}
			};

			return Converter.Convert(doc);
		}
	}
}
