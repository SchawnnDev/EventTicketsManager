using System;

namespace EventTicketsManager.Models;

public class TicketScanStatsModel
{
    
    public int TicketsCount { get; set; }

    public int ScannedTicketsCount { get; set; }

    public int PayedTicketsCount { get; set; }

    public int ScannedPayedTicketsCount { get; set; }
    
    public TicketScanStatsModel()
    {
        TicketsCount = 0;
        ScannedTicketsCount = 0;
        PayedTicketsCount = 0;
        ScannedPayedTicketsCount = 0;
    }
    
    public int CalculateScannedTicketsCountPercentage()
    {
        if (TicketsCount == 0)
            return 0;
        return (int) Math.Round((double) ScannedTicketsCount / TicketsCount * 100);
    }
    
    public int CalculateScannedPayedTicketsCountPercentage()
    {
        if (PayedTicketsCount == 0)
            return 0;
        return (int) Math.Round((double) ScannedPayedTicketsCount / TicketsCount * 100);
    }
    
    public int CalculatePayedTicketsCountPercentage()
    {
        if (TicketsCount == 0)
            return 0;
        return (int) Math.Round((double) PayedTicketsCount / TicketsCount * 100);
    }
    
}