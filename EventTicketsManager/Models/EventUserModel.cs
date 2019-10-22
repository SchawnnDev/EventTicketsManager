using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Server;

namespace EventTicketsManager.Models
{
    public class EventUserModel
    {

        public SaveableEventUser EventUser { get; set; }

        public string Email { get; set; }

        public EventUserModel(SaveableEventUser eventUser, string email)
        {
            EventUser = eventUser;
            Email = email;
        }

    }
}
