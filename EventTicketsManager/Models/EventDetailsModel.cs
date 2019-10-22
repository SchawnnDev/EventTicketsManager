using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Server;

namespace EventTicketsManager.Models
{
    public class EventDetailsModel
    {

        public SaveableEvent Event { get; set; }
        public List<EventUserModel> EventUsers { get; set; }

        public EventDetailsModel() { }

        public EventDetailsModel(SaveableEvent saveableEvent, List<EventUserModel> eventUsers)
        {
            Event = saveableEvent;
            EventUsers = eventUsers;
        }

    }
}
