using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Server;

namespace Library.Utils
{
    public class DbUtils
    {

        public static bool IsEventExisting(int id, ServerContext context) => context.Events.Any(t => t.Id == id);

        public static bool IsTicketExisting(int id, ServerContext context) => context.Tickets.Any(t => t.Id == id);

        public static bool IsUserEventOwner(int id, string playerId, ServerContext context) =>
            context.Events.Any(t => t.Id == id && t.CreatorId.Equals(playerId));

        public static bool IsUserEventCollaborator(int id, string playerId, ServerContext context) =>
            context.EventUsers.Any(t => t.Event.Id == id && t.UserId.Equals(playerId));

        public static bool IsEventExistingAndUserEventMember(int id, string playerId, ServerContext context) =>
            IsUserEventOwner(id, playerId, context) || IsUserEventCollaborator(id, playerId, context);

    }
}
