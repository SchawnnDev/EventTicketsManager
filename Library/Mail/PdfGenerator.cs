using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DinkToPdf;
using Library.Api;
using QRCoder;
using Server;

namespace Library.Mail
{
	public class PdfGenerator
	{

		private readonly SynchronizedConverter Converter;
		private readonly SaveableTicketQrCode QrCode;

		public PdfGenerator(SaveableTicketQrCode qrCode)
		{
			Converter = new SynchronizedConverter(new PdfTools());
		}

		private string GenerateQrCode()
		{
			var generator = new QrCodeGenerator(QrCode);
			using var qrGenerator = new QRCodeGenerator();
			var qrCodeData = qrGenerator.CreateQrCode(generator.Get(), QRCodeGenerator.ECCLevel.H);
			var qrCode = new SvgQRCode(qrCodeData);
			return qrCode.GetGraphic(64);
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
						HtmlContent = GenerateQrCode(),
						WebSettings = { DefaultEncoding = "utf-8" },
					//	HeaderSettings = { FontSize = 9, Right = "Page [page] of [toPage]", Line = true, Spacing = 2.812 }
					}
				}
			};

			return Converter.Convert(doc);
		}
	}
}
