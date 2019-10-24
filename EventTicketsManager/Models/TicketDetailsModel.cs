using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public TicketDetailsModel(SaveableTicket ticket, List<SaveableTicketScan> scans, List<SaveableTicketUserMail> mails, string creatorEmail)
        {
            Ticket = ticket;
			Scans = scans;
            TicketScanNumber = scans.Count;
            TicketScanned =  TicketScanNumber != 0;
            Mails = mails;
            MailSent = MailsSent != 0;
            MailsSent = mails.Count;
            CreatorEmail = creatorEmail;
        }

    }
}
