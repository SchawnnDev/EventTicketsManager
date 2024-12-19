using System;
using System.Collections.Generic;
using System.Text;
using Library.Enums;
using Library.Utils;
using Server;

namespace EventTicketsManager.Models;

public class EventStatisticsModel
{
    public SaveableEvent Event { get; set; }
    public List<DateTime> TicketsByDate { get; set; }
    public int TicketsCount { get; set; }
    public int TicketsPayedCount { get; set; }
    public decimal TicketsTotalValue { get; set; }
    public Dictionary<Gender, int> TicketsGender { get; set; }
    public Dictionary<string, int> TicketsCreator { get; set; }
    public Dictionary<PaymentMethod, int> TicketsPaymentMethod { get; set; }
    public decimal TicketsPayedTotalValue { get; set; }
    public Dictionary<string, Tuple<decimal, decimal, decimal>> TicketsValueByCreator { get; set; }
    public Dictionary<decimal, int> TicketsPrice { get; set; }
    
    public string GenTicketsByDate()
    {
        var builder = new StringBuilder("[");
        var i = 1;

        foreach (var item in TicketsByDate)
        {
            builder.Append("[");
            builder.Append($"{item.ToUnixTimeStamp().ToString()}000");
            builder.Append(",");
            builder.Append(1);
            builder.Append("]");
            if (i++ != TicketsByDate.Count)
                builder.Append(",");
        }

        return builder.Append("]").ToString();
    }
}