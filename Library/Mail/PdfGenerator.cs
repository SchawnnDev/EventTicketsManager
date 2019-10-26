using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DinkToPdf;

namespace Library.Mail
{
	public class PdfGenerator
	{

		private readonly SynchronizedConverter Converter;

		public PdfGenerator()
		{
			Converter = new SynchronizedConverter(new PdfTools());
		}

		public byte[] Generate()
		{
			var doc = new HtmlToPdfDocument()
			{
				GlobalSettings = {
					ColorMode = ColorMode.Color,
					Orientation = Orientation.Landscape,
					PaperSize = PaperKind.A4Plus,
					Out = Path.GetTempFileName()

		},
				Objects = {
					new ObjectSettings() {
						PagesCount = true,
						HtmlContent = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. In consectetur mauris eget ultrices  iaculis. Ut                               odio viverra, molestie lectus nec, venenatis turpis.",
						WebSettings = { DefaultEncoding = "utf-8" },
						HeaderSettings = { FontSize = 9, Right = "Page [page] of [toPage]", Line = true, Spacing = 2.812 }
					}
				}
			};

			return Converter.Convert(doc);
		}
	}
}
