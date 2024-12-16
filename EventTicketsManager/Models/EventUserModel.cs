using Server;

namespace EventTicketsManager.Models;

public class EventUserModel
{
    public EventUserModel(SaveableEventUser eventUser, string email)
    {
        EventUser = eventUser;
        Email = email;
    }

    public SaveableEventUser EventUser { get; set; }

    public string Email { get; set; }
}