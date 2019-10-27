using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Library.Api;
using Library.Utils;
using Server;

namespace EventTicketsManager.Models
{
    public class TicketDetailsModel
    {

        public SaveableTicket Ticket { get; set; }

        public bool TicketScanned { get; set; }

        public int TicketScanNumber { get; set; }

		public bool MailSent { get; set; }

		public int MailsSent { get; set; }

        public string CreatorEmail { get; set; }

		public List<SaveableTicketScan> Scans { get; set; }

		public List<SaveableTicketUserMail> Mails { get; set; }

		public SaveableTicketQrCode QrCode { get; set; }
		
		public string Error { get; set; }

		public TicketDetailsModel(SaveableTicket ticket, List<SaveableTicketScan> scans, List<SaveableTicketUserMail> mails, SaveableTicketQrCode qrCode, string creatorEmail, string error)
        {
            Ticket = ticket;
			Scans = scans;
            TicketScanNumber = scans.Count;
            TicketScanned =  TicketScanNumber != 0;
            Mails = mails;
            MailSent = MailsSent != 0;
            MailsSent = mails.Count;
            CreatorEmail = creatorEmail;
            QrCode = qrCode;
            Error = error;
        }

		public bool HasQrCode() => QrCode != null;

		public string GetQrCodeContent() => HasQrCode()
			? new QrCodeGenerator(QrCode).Get()
			: "";

		public bool IsError() => !string.IsNullOrEmpty(Error);

	}
}
