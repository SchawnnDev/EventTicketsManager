using System;
using Server;

namespace EventTicketsManager.Models;

public class TicketScanModel
{
    
    public string Error { get; set; }
    
    public bool Scanned { get; set; }
    
    public SaveableTicket Ticket { get; set; }
    
    public DateTime? LastScan { get; set; }
    
    public TicketScanStatsModel Stats { get; set; }
    
    public bool IsError() => Error != null;

    public int EventId { get; }
    
    public TicketScanModel(int eventId)
    {
        EventId = eventId;
    }
}