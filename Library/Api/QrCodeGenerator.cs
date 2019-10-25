using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Library.Utils;
using Server;

namespace Library.Api
{
	public class QrCodeGenerator
	{
		private SaveableTicket Ticket { get; set; }

		private SaveableTicketQrCode QrCode { get; set; }

		private byte[] Encrypted { get; set; }

		public QrCodeGenerator(SaveableTicket ticket) => Ticket = ticket;

		public QrCodeGenerator(SaveableTicketQrCode qrCode) => QrCode = qrCode;

		public SaveableTicketQrCode GenerateKeys()
		{
			if (Ticket == null) return null;
			using var myAes = Aes.Create();
			// Encrypt the string to an array of bytes.
			Encrypted = AESEncryption.EncryptStringToBytes($"{Ticket.Id}§§{Ticket.Email}", myAes?.Key, myAes?.IV);

			return new SaveableTicketQrCode
			{
				IV = myAes?.IV,
				Key = myAes?.Key,
				CreatedAt = DateTime.Now,
				Ticket = Ticket
			};

		}

		public string Generate(int id) => Encrypted != null ? $"{id}§§{Convert.ToBase64String(Encrypted)}" : "";

		public string Get()
		{
			if (QrCode == null) return "";
			Encrypted = AESEncryption.EncryptStringToBytes($"{QrCode.Ticket.Id}§§{QrCode.Ticket.Email}", QrCode.Key, QrCode.IV);
			return $"{QrCode.Id}§§{Convert.ToBase64String(Encrypted)}";
		}

	}
}
