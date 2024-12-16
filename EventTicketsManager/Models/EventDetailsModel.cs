using System.Collections.Generic;
using Library.Enums;
using Server;

namespace EventTicketsManager.Models;

public class EventDetailsModel
{
    public EventDetailsModel()
    {
    }

    public EventDetailsModel(SaveableEvent saveableEvent, List<EventUserModel> eventUsers)
    {
        Event = saveableEvent;
        EventUsers = eventUsers;
    }

    public SaveableEvent Event { get; set; }
    public List<EventUserModel> EventUsers { get; set; }
    public bool Error { get; set; }
    public string Message { get; set; }

    public int TicketsCreated { get; set; }

    public int TicketsMailSent { get; set; }

    public int TicketsScanned { get; set; }

    public Dictionary<Gender, int> TicketsGenreCount { get; set; }

    public bool HasMessage()
    {
        return !string.IsNullOrWhiteSpace(Message);
    }

    public string TicketsGenreCountString()
    {
        return
            $"F: {TicketsGenreCount[Gender.Female]} | M: {TicketsGenreCount[Gender.Male]} | D: {TicketsGenreCount[Gender.Diverse]}";
    }
}