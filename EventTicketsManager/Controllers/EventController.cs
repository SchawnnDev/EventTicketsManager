using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DinkToPdf.Contracts;
using EventTicketsManager.Models;
using EventTicketsManager.Services;
using Library;
using Library.Api;
using Library.Enums;
using Library.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Server;

namespace EventTicketsManager.Controllers
{
    [Authorize]
    public class EventController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IConverter _converter;

        public EventController(IConfiguration configuration, IConverter converter,
            UserManager<IdentityUser> userManager)
        {
            _configuration = configuration;
            _converter = converter;
            _userManager = userManager;
        }

        // GET: Event
        public ActionResult Index()
        {
            var events = new List<SaveableEvent>();
            var userId = _userManager.GetUserId(User);

            using (var db = new ServerContext())
            {
                events.AddRange(db.Events.Where(t => t.CreatorId == userId).ToList());

                var list = db.EventUsers.Where(t => t.UserId == userId)
                    .Select(t => t.Event).ToList();

                foreach (var saveableEvent in list)
                {
                    saveableEvent.IsCreator = false;

                    if (events.All(t => t.Id != saveableEvent.Id))
                        events.Add(saveableEvent);
                }
            }

            return View("Index", events);
        }

        // GET: Event/Details/5
        public ActionResult Details(int id) => Details(id, null, false);

        private ActionResult Details(int id, string message, bool error)
        {
            var model = new EventDetailsModel();

            using (var db = new ServerContext())
            {
                if (db.Events.Any(t => t.Id == id))
                    model.Event = db.Events.Find(id);
                model.EventUsers = new List<EventUserModel>(db.EventUsers.Where(t => t.Event.Id == id).ToList()
                    .Select(t => new EventUserModel(t,
                        db.Users.Where(e => e.Id == t.UserId).Select(e => e.Email).Single() ?? "null@null.null"))
                    .ToList());

                model.TicketsScanned = db.TicketScans.Count(t => t.Ticket.Event.Id == id);
                model.TicketsMailSent = db.TicketUserMails.Count(t => t.Ticket.Event.Id == id);
                model.TicketsCreated = db.Tickets.Count(t => t.Event.Id == id);
                model.TicketsGenreCount = new Dictionary<Gender, int>();

                foreach (Gender gender in Enum.GetValues(typeof(Gender)))
                {
                    var genderInt = (int) gender;
                    model.TicketsGenreCount.Add(gender,
                        db.Tickets.Count(t => t.Event.Id == id && t.Gender == genderInt));
                }
            }

            model.Message = message;
            model.Error = error;

            return model.Event != null ? View("Details", model) : View("Index");
        }

        // GET: Event/Create
        public ActionResult Create()
        {
            var date = DateTime.UtcNow;
            date = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Kind);
            return View(new SaveableEvent {Start = date, End = date.AddDays(1)});
        }

        // POST: Event/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                var saveableEvent = new SaveableEvent();

                FillEvent(saveableEvent, collection);

                saveableEvent.CreatorId = _userManager.GetUserId(User);
                saveableEvent.CreatedAt = DateTime.UtcNow;
                saveableEvent.UpdatedAt = DateTime.UtcNow;
                saveableEvent.ApiKey = new KeyGenerator(saveableEvent).GenerateNewKey();

                using (var db = new ServerContext())
                {
                    db.Events.Add(saveableEvent);
                    db.SaveChanges();
                }


                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Event/Edit/5
        public ActionResult Edit(int id)
        {
            SaveableEvent model = null;

            using (var db = new ServerContext())
            {
                if (db.Events.Any(t => t.Id == id))
                    model = db.Events.Find(id);
            }

            return model != null ? View(model) : View("Index");
        }

        public ActionResult Stats(int id)
        {
            EventStatisticsModel model = null;
            var sqlMinDate = (DateTime) SqlDateTime.MinValue;

            using (var db = new ServerContext())
            {
                if (db.Events.Any(t => t.Id == id))
                {
                    model = new EventStatisticsModel
                    {
                        Event = db.Events.Find(id),
                        TicketsCount = db.Tickets.Count(t => t.Event.Id == id),
                        TicketsPayedCount = db.Tickets.Count(t => t.HasPaid && t.Event.Id == id),
                        TicketsGender = db.Tickets.Where(t => t.Event.Id == id).GroupBy(t => t.Gender)
                            .Select(t => new {gender = (Gender) t.Key, count = t.Count()})
                            .ToDictionary(t => t.gender, t => t.count),
                        TicketsPaymentMethod = db.Tickets.Where(t => t.Event.Id == id).GroupBy(t => t.PaymentMethod)
                            .Select(t => new {paymentMethod = (PaymentMethod) t.Key, count = t.Count()})
                            .ToDictionary(t => t.paymentMethod, t => t.count),
                        //TicketsByDate = db.Tickets.Where(t => t.Event.Id == id).OrderBy(t => t.CreatedAt).Select(t => t.CreatedAt).ToList(),
                        TicketsTotalValue = db.Tickets.Where(t => t.Event.Id == id).Sum(t => t.ToPay),
                        TicketsPayedTotalValue = db.Tickets.Where(t => t.Event.Id == id && t.HasPaid).Sum(t => t.ToPay),
                        TicketsCreator = db.Tickets.Where(t => t.Event.Id == id).GroupBy(t => t.CreatorId).Select(t =>
                                new
                                {
                                    email = db.Users.Where(e => e.Id == t.Key).Select(e => e.Email).First(),
                                    count = t.Count()
                                })
                            .ToDictionary(t => t.email, t => t.count),
                        /*       TicketsByDate = db.Tickets
                                    .Where(t=>t.Event.Id == id)
                                    .GroupBy(x => SqlFunctions.DateAdd("month",
                                        SqlFunctions.DateDiff("month", sqlMinDate, x.CreatedAt), sqlMinDate))
                                    .Select(t=> new {date=t.Key, count=t.Count()})
                                    .ToDictionary(t => t.date, t => t.count) */
                        TicketsValueByCreator = db.Tickets.Where(t => t.Event.Id == id).GroupBy(t => t.CreatorId)
                            .Select(t => new
                            {
                                email = db.Users.Where(e => e.Id == t.Key).Select(e => e.Email).First(),
                                notPaid = t.Where(e => !e.HasPaid).Sum(e => e.ToPay),
                                paid = t.Where(e => e.HasPaid).Sum(e => e.ToPay)
                            })
                            .ToDictionary(t => t.email, t => new Tuple<int, int>((int) t.notPaid, (int) t.paid))
                    };
                }
            }


            return model != null ? View("Statistics", model) : Details(id, "Error trying to display stats.", true);
        }

        // POST: Event/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                using (var db = new ServerContext())
                {
                    if (!db.Events.Any(t => t.Id == id)) // Check if the event exists
                        return View("Index");

                    var saveableEvent = db.Events.Single(t => t.Id == id);

                    if (saveableEvent.CreatorId != _userManager.GetUserId(User)
                    ) // Check if editor is owner of the event.
                        return View("Index");

                    FillEvent(saveableEvent, collection);

                    saveableEvent.UpdatedAt = DateTime.UtcNow;

                    db.SaveChanges();
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                return View("Index");
            }
        }

        private void FillEvent(SaveableEvent saveableEvent, IFormCollection collection)
        {
            if (collection.TryGetValue("Name", out var name))
                saveableEvent.Name = name;
            if (collection.TryGetValue("AddressName", out var addressName))
                saveableEvent.AddressName = addressName;
            if (collection.TryGetValue("AddressNumber", out var addressNumber))
                saveableEvent.AddressNumber = addressNumber;
            if (collection.TryGetValue("CityName", out var cityName))
                saveableEvent.CityName = cityName;
            if (collection.TryGetValue("PostalCode", out var postalCode))
                saveableEvent.PostalCode = postalCode;
            if (collection.TryGetValue("TelephoneNumber", out var telephoneNumber))
                saveableEvent.TelephoneNumber = telephoneNumber;
            if (collection.TryGetValue("Email", out var email))
                saveableEvent.Email = email;
            if (collection.TryGetValue("LogoUrl", out var logoUrl))
                saveableEvent.LogoUrl = logoUrl;
            if (collection.TryGetValue("HeaderUrl", out var headerUrl))
                saveableEvent.HeaderUrl = headerUrl;
            if (collection.TryGetValue("EmailContent", out var emailContent))
                saveableEvent.EmailContent = emailContent;
            if (collection.TryGetValue("EnterPrice", out var enterPrice))
                saveableEvent.EnterPrice = Math.Max(0, decimal.Parse(enterPrice));
            if (collection.TryGetValue("Start", out var start))
                saveableEvent.Start = DateTime.Parse(start);
            if (collection.TryGetValue("End", out var end))
                saveableEvent.End = DateTime.Parse(end);
            if (collection.TryGetValue("Enabled", out var enabled))
                saveableEvent.Enabled = bool.Parse(enabled.ToString().Split(",")[0]);
        }

        // GET: Event/Delete/5
        public ActionResult Delete(int id)
        {
            /*
            using var db = new ServerContext();
            return View(db.Events.Single(t => t.Id == id)); */

            return Details(id);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        // GET: Event/Delete/5
        public ActionResult ExportTicketsCsv(int id, IFormCollection collection)
        {

            var list = new List<string>();
            var header = new List<string>();

            if (collection.ContainsKey("firstName"))
                header.Add("Prénom");
            if (collection.ContainsKey("name"))
                header.Add("Nom");
            if (collection.ContainsKey("email"))
                header.Add("Email");
            if (collection.ContainsKey("hasPaid"))
                header.Add("Payé");
            if (collection.ContainsKey("toPay"))
                header.Add("Montant");
            if (collection.ContainsKey("paymentMethod"))
                header.Add("MoyenPaiement");
            if (collection.ContainsKey("scanned"))
                header.Add("Scanné");
            if (collection.ContainsKey("scannedAT"))
                header.Add("Scanné le");
            if (collection.ContainsKey("updatedAt"))
                header.Add("DateMaj");
            if (collection.ContainsKey("createdAt"))
                header.Add("DateCreation");

            using (var db = new ServerContext())
            {
                foreach (var ticket in db.Tickets.Where(t=>t.Event.Id == id).ToList())
                {//firstName,name,email,hasPaid,toPay,paymentMethod,scanned,updatedAt,createdAt

                    var ticketVal = new List<string>();

                    if (collection.ContainsKey("firstName"))
                        ticketVal.Add(ticket.FirstName);
                    if(collection.ContainsKey("name"))
                        ticketVal.Add(ticket.LastName);
                    if (collection.ContainsKey("email"))
                        ticketVal.Add(ticket.Email);
                    if (collection.ContainsKey("hasPaid"))
                        ticketVal.Add(ticket.HasPaid ? "oui" : "non");
                    if (collection.ContainsKey("toPay"))
                        ticketVal.Add(ticket.ToPay.ToString("C", CultureInfo.CreateSpecificCulture("fr-FR")).Replace(",","."));
                    if (collection.ContainsKey("paymentMethod"))
                        ticketVal.Add(((PaymentMethod)ticket.PaymentMethod).ToString().ToLower());
                    if (collection.ContainsKey("scanned"))
                        ticketVal.Add(db.TicketScans.Any(t=>t.Ticket.Id == ticket.Id) ? "oui" : "non");
                    if (collection.ContainsKey("scannedAt"))
                        ticketVal.Add(db.TicketScans.Any(t => t.Ticket.Id == ticket.Id) ? db.TicketScans.First(t=>t.Ticket.Id == ticket.Id).Date.ToString("g", CultureInfo.CreateSpecificCulture("fr-FR")) : "non scanné");
                    if (collection.ContainsKey("updatedAt"))
                        ticketVal.Add(ticket.UpdatedAt.ToString("g", CultureInfo.CreateSpecificCulture("fr-FR")));
                    if (collection.ContainsKey("createdAt"))
                        ticketVal.Add(ticket.CreatedAt.ToString("g", CultureInfo.CreateSpecificCulture("fr-FR")));

                    list.Add(string.Join(',',ticketVal));
                }
            }

            // Response...
            System.Net.Mime.ContentDisposition cd = new System.Net.Mime.ContentDisposition
            {
                FileName = "exportedTickets.csv",
                Inline = false  // false = prompt the user for downloading;  true = browser to try to show the file inline
            };
            Response.Headers.Append("Content-Disposition", cd.ToString());
            Response.Headers.Append("X-Content-Type-Options", "nosniff");

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(string.Join(',', header));
            foreach (var line in list)
                stringBuilder.AppendLine(line);

            return File(Encoding.Unicode.GetBytes(stringBuilder.ToString()), "text/csv");

        }

        // POST: Event/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // POST: Event/DeleteEventUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteEventUser(IFormCollection collection)
        {
            var eventId = 0;
            int id;

            try
            {
                using var db = new ServerContext();

                if (collection.TryGetValue("Id", out var aId))
                {
                    if (!string.IsNullOrWhiteSpace(aId) && int.TryParse(aId, out var eId))
                        id = eId;
                    else
                        return Index();
                }
                else return Index();

                if (collection.TryGetValue("EventId", out var strId))
                {
                    if (!string.IsNullOrWhiteSpace(strId) && int.TryParse(strId, out var eId))
                        eventId = eId;
                    else
                        return Index();
                }
                else return Index();

                if (!db.Events.Any(t => t.Id == eventId && t.CreatorId == _userManager.GetUserId(User)) ||
                    !db.EventUsers.Any(t => t.Id == id && t.Event.Id == eventId))
                    return Details(eventId);

                var user = db.EventUsers.Single(t => t.Id == id);

                if (user == null) return Details(eventId);

                db.Remove(user);
                db.SaveChanges();
            }
            catch (Exception)
            {
                return Details(eventId);
            }

            return Details(eventId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddEventUser(IFormCollection collection)
        {
            var id = 0;

            try
            {
                string email;

                using (var db = new ServerContext())
                {
                    if (collection.TryGetValue("EventId", out var strId))
                    {
                        if (!string.IsNullOrWhiteSpace(strId) && int.TryParse(strId, out var eventId))
                            id = eventId;
                        else
                            return Index();
                    }
                    else return Index();


                    if (collection.TryGetValue("Email", out var parsedEmail))
                        email = parsedEmail.ToString();
                    else
                        return Details(id);

                    if (!db.Events.Any(t => t.Id == id) || !db.Users.Any(t => t.Email.Equals(email)))
                        return Details(id);

                    var saveableEvent = db.Events.Single(t => t.Id == id);

                    var ownerEmail = db.Users.Where(t => t.Id == saveableEvent.CreatorId).Select(t => t.Email).Single();

                    if (string.IsNullOrWhiteSpace(ownerEmail) || email.Equals(ownerEmail))
                        return Details(id);
                    var userId = db.Users.Where(t => t.Email == email).Select(t => t.Id).Single();

                    if (db.EventUsers.Any(t => t.Event.Id == id && t.UserId == userId))
                        return Details(id);

                    var user = new SaveableEventUser
                    {
                        Event = saveableEvent,
                        UserId = userId
                    };

                    db.EventUsers.Add(user);

                    db.SaveChanges();
                }

                return Details(id);
            }
            catch (Exception)
            {
                return Details(id);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RenewApiKey(IFormCollection collection)
        {
            var eventId = 0;

            try
            {
                using var db = new ServerContext();

                if (collection.TryGetValue("EventId", out var strId))
                {
                    if (!string.IsNullOrWhiteSpace(strId) && int.TryParse(strId, out var id))
                        eventId = id;
                    else
                        return Index();
                }
                else return Index();

                if (!db.Events.Any(t => t.Id == eventId && t.CreatorId == _userManager.GetUserId(User)))
                    return Details(eventId);

                var saveableEvent = db.Events.Single(t => t.Id == eventId);

                saveableEvent.ApiKey = new KeyGenerator(saveableEvent).GenerateNewKey();

                db.SaveChanges();

                return Details(eventId);
            }
            catch (Exception)
            {
                return Details(eventId);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendTicketsNotSent(int id, IFormCollection collection)
        {
            try
            {
                await using var db = new ServerContext();

                if (!DbUtils.IsEventExistingAndUserEventMember(id, _userManager.GetUserId(User), db))
                    return Details(id, "Event is not existing or you're not owner/collaborator of this event", true);

                var saveableEvent = db.Events.Find(id);
                var notSentTickets = db.Tickets
                    .Where(t => t.Event.Id == id && !db.TicketUserMails.Any(e => e.Ticket.Id == t.Id)).ToList();
                var qrCodes = new List<SaveableTicketQrCode>();

                foreach (var notSentTicket in notSentTickets)
                {
                    if (db.QrCodes.Any(t => t.Ticket.Id == notSentTicket.Id))
                    {
                        qrCodes.Add(db.QrCodes.Where(t => t.Ticket.Id == notSentTicket.Id).Include(t => t.Ticket)
                            .Single());
                    }
                    else
                    {
                        var qrCode = new QrCodeGenerator(notSentTicket);

                        var saveableQrCode = qrCode.GenerateKeys();

                        saveableQrCode.CreatorId = _userManager.GetUserId(User);

                        db.Tickets.Attach(saveableQrCode.Ticket);

                        qrCodes.Add(saveableQrCode);
                    }
                }

                db.QrCodes.AddRange(qrCodes.Where(t => t.Id == 0).ToList());

                await db.SaveChangesAsync(); // Save to receive ID

                var multipleMailGenerator = new MultipleMailGenerator(saveableEvent,
                    qrCodes.Select(t => new MultipleMail {QrCode = t, Ticket = t.Ticket}).ToList(), _converter, db,
                    _configuration["SendGridKey"], _userManager.GetUserId(User));

                try
                {
                    await multipleMailGenerator.SendAllMailsAsync();
                }
                catch (OperationCanceledException ex)
                {
                    return Details(id, ex.Message, true);
                }

                await db.SaveChangesAsync();

                return Details(id, $"{qrCodes.Count} emails were successfully sent!", false);
            }
            catch (Exception e)
            {
                return Details(id, e.Message, true);
            }
        }
    }
}