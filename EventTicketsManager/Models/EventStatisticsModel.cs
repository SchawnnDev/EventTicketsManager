using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Library.Enums;
using Server;

namespace EventTicketsManager.Models
{
    public class EventStatisticsModel
    {

        public SaveableEvent Event { get; set; }
        public Dictionary<DateTime?, int> TicketsByDate { get; set; }
        public int TicketsCount { get; set; }
        public int TicketsPayedCount { get; set; }
        public Dictionary<Gender, int> TicketsGender { get; set; }
    }

}
