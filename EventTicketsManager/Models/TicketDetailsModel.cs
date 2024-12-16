using System.Collections.Generic;
using Library.Api;
using Server;

namespace EventTicketsManager.Models;

public class TicketDetailsModel
{
    public TicketDetailsModel(SaveableTicket ticket, List<SaveableTicketScan> scans, List<SaveableTicketUserMail> mails,
        SaveableTicketQrCode qrCode, string creatorEmail, string error)
    {
        Ticket = ticket;
        Scans = scans;
        TicketScanNumber = scans.Count;
        TicketScanned = TicketScanNumber != 0;
        Mails = mails;
        MailsSent = mails.Count;
        MailSent = MailsSent != 0;
        CreatorEmail = creatorEmail;
        QrCode = qrCode;
        Error = error;
    }

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

    public bool HasQrCode()
    {
        return QrCode != null;
    }

    public string GetQrCodeContent()
    {
        return HasQrCode()
            ? new QrCodeGenerator(QrCode).Get()
            : "";
    }

    public bool IsError()
    {
        return !string.IsNullOrEmpty(Error);
    }
}