using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DinkToPdf.Contracts;
using EventTicketsManager.Models;
using EventTicketsManager.Services;
using Library;
using Library.Api;
using Library.Pdf;
using Library.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Server;

namespace EventTicketsManager.Controllers
{
    [Authorize]
    public class TicketController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        private readonly IConfiguration _configuration;

        private readonly IConverter _converter;

        public TicketController(IConfiguration configuration, IConverter converter,
            UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
            _configuration = configuration;
            _converter = converter;
        }

        // GET: User
        public ActionResult Index()
        {
            return Index(null);
        }

        private ActionResult Index(string error)
        {
            var events = new List<SaveableEvent>();
            var userId = _userManager.GetUserId(User);

            using (var db = new ServerContext())
            {
                events.AddRange(db.Events.Where(t => t.CreatorId == userId && t.Enabled).ToList());

                var list = db.EventUsers.Where(t => t.UserId == userId && t.Event.Enabled)
                    .Select(t => t.Event).ToList();

                foreach (var saveableEvent in list.Where(saveableEvent => events.All(t => t.Id != saveableEvent.Id)))
                {
                    saveableEvent.IsCreator = false;

                    if (events.All(t => t.Id != saveableEvent.Id))
                        events.Add(saveableEvent);
                }
            }


            return View("Index", new TicketIndexModel(events, error));
        }

        public ActionResult Details(int id)
        {
            return Details(id, null);
        }

        // GET: User/Details/5
        private ActionResult Details(int id, string error)
        {
            SaveableTicket ticket;
            string creatorEmail;
            List<SaveableTicketScan> scans;
            List<SaveableTicketUserMail> mails;
            SaveableTicketQrCode qrCode;

            using (var db = new ServerContext())
            {
                if (!DbUtils.IsTicketExisting(id, db)) return List(id, "This ticket does not exists.");

                ticket = db.Tickets.Include(t => t.Event).Single(t => t.Id == id);
                creatorEmail = db.Users.Any(t => t.Id == ticket.CreatorId)
                    ? db.Users.Where(t => t.Id == ticket.CreatorId).Select(t => t.Email).Single()
                    : "Not exists anymore";
                scans = db.TicketScans.Where(t => t.Ticket.Id == id).OrderBy(t => t.Date).Select(t =>
                    new SaveableTicketScan(t.Ticket,
                        db.Users.Any(e => e.Id == t.CreatorId)
                            ? db.Users.Where(e => e.Id == t.CreatorId).Select(e => e.Email).Single()
                            : "User doesn't exist anymore.'", t.Date)).ToList();
                mails = db.TicketUserMails.Where(t => t.Ticket.Id == id).OrderBy(t => t.Date).Select(t =>
                    new SaveableTicketUserMail(t.Ticket,
                        db.Users.Any(e => e.Id == t.CreatorId)
                            ? db.Users.Where(e => e.Id == t.CreatorId).Select(e => e.Email).Single()
                            : "User doesn't exist anymore.'", t.Date)).ToList();
                qrCode = db.QrCodes.SingleOrDefault(t => t.Ticket.Id == id);
            }

            return View("Details", new TicketDetailsModel(ticket, scans, mails, qrCode, creatorEmail, error));
        }

        // GET: User/Create
        public ActionResult Create(int id)
        {
            decimal toPay;
            using (var db = new ServerContext())
            {
                if (!DbUtils.IsEventExisting(id, db)) return Index("Event not existing");
                toPay = db.Events.Where(t => t.Id == id).Select(t => t.EnterPrice).Single();
            }

            return View(new TicketCreateModel(new SaveableTicket { TicketEventId = id, ToPay = toPay }, null));
        }

        private ActionResult Create(SaveableTicket ticket, string error)
        {
            return View(new TicketCreateModel(ticket, error));
        }

        // POST: User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                int eventId;

                if (collection.TryGetValue("TicketEventId", out var ticketEventIdStr))
                {
                    if (int.TryParse(ticketEventIdStr, out var ticketEventId))
                        eventId = ticketEventId;
                    else return Index("Can't parse ticket event id");
                }
                else return Index("Can't find ticket event id input");

                var saveableTicket = new SaveableTicket();

                using (var db = new ServerContext())
                {
                    var user = _userManager.GetUserId(User);


                    if (!DbUtils.IsEventExistingAndUserEventMember(eventId, _userManager.GetUserId(User), db))
                        return List(eventId, "event is not existing or you're not owner/collaborator of this event");

                    FillTicket(saveableTicket, collection);

                    if (collection.TryGetValue("Email", out var email))
                    {
                        if (db.Tickets.Any(t =>
                                t.Event.Id == eventId && t.Email.ToLower().Equals(email.ToString().ToLower())))
                        {
                            saveableTicket.TicketEventId = eventId;
                            return Create(saveableTicket, "This email is already taken!");
                        }

                        saveableTicket.Email = email.ToString();
                    }
                    else
                    {
                        return List(eventId, "Can't get email!");
                    }

                    if (!MailUtils.IsEmailValid(saveableTicket.Email))
                        return Create(saveableTicket, "Email is not valid!");

                    var saveableEvent = db.Events.Find(eventId);

                    saveableTicket.Event = saveableEvent;
                    saveableTicket.CreatorId = saveableTicket.UpdaterId = user;
                    saveableTicket.CreatedAt = saveableTicket.UpdatedAt = DateTime.UtcNow;

                    db.Tickets.Add(saveableTicket);

                    Logger.SendLog($"Created ticket for {saveableTicket.Email}", user, db);
                    db.SaveChanges();
                }


                return List(eventId);
            }
            catch (Exception e)
            {
                return Index(e.Message);
            }
        }

        // GET: User/Edit/5
        public ActionResult Edit(int id)
        {
            SaveableTicket model = null;

            using (var db = new ServerContext())
            {
                if (DbUtils.IsTicketExisting(id, db))
                    model = db.Tickets.Find(id);
            }

            return model != null ? View(model) : Index("Ticket does not exists.");
        }

        // POST: User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                int eventId;

                using (var db = new ServerContext())
                {
                    if (!DbUtils.IsTicketExisting(id, db)) return Index("Ticket does not exists.");

                    var saveableTicket = db.Tickets.Include(t => t.Event).Single(t => t.Id == id);

                    eventId = saveableTicket.Event.Id;

                    if (!DbUtils.IsEventExistingAndUserEventMember(eventId, _userManager.GetUserId(User), db))
                        return List(eventId, "Event is not existing or you're not owner/collaborator of this event");

                    FillTicket(saveableTicket, collection);

                    saveableTicket.UpdaterId = _userManager.GetUserId(User);
                    saveableTicket.UpdatedAt = DateTime.UtcNow;

                    db.Events.Attach(saveableTicket.Event);

                    Logger.SendLog($"Edited ticket n°{saveableTicket.Id}", _userManager.GetUserId(User), db);
                    db.SaveChanges();
                }


                return List(eventId);
            }
            catch (Exception)
            {
                return Index();
            }
        }

        private void FillTicket(SaveableTicket saveableTicket, IFormCollection collection)
        {
            if (collection.TryGetValue("FirstName", out var firstName))
                saveableTicket.FirstName = firstName;
            if (collection.TryGetValue("LastName", out var lastName))
                saveableTicket.LastName = lastName;
            if (collection.TryGetValue("Gender", out var genderStr))
                if (int.TryParse(genderStr, out var gender))
                    saveableTicket.Gender = gender;
            if (collection.TryGetValue("PaymentMethod", out var paymentMethodStr))
                if (int.TryParse(paymentMethodStr, out var paymentMethod))
                    saveableTicket.PaymentMethod = paymentMethod;
            if (collection.TryGetValue("ToPay", out var toPayStr))
                if (decimal.TryParse(toPayStr, out var toPay))
                    saveableTicket.ToPay = toPay;
            if (!collection.TryGetValue("HasPaid", out var hasPaidStr)) return;
            if (int.TryParse(hasPaidStr, out var hasPaid))
                saveableTicket.HasPaid = hasPaid == 1;
        }


        // GET: User/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: User/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            // var eventId = 0;

            try
            {
                using var db = new ServerContext();

                if (!DbUtils.IsTicketExisting(id, db))
                    return Details(id, "Ticket does not exists.");
                var ticket = db.Tickets.Include(t => t.Event).Single(t => t.Id == id);

                if (!DbUtils.IsEventExistingAndUserEventMember(ticket.Event.Id, _userManager.GetUserId(User), db))
                    return Details(id, "Event is not existing or you're not owner/collaborator of this event");


                if (db.QrCodes.Any(t => t.Ticket.Id == id))
                    db.QrCodes.Remove(db.QrCodes.Single(t => t.Ticket.Id == id));

                db.TicketUserMails.RemoveRange(db.TicketUserMails.Where(t => t.Ticket.Id == id).ToArray());

                db.TicketScans.RemoveRange(db.TicketScans.Where(t => t.Ticket.Id == id).ToArray());

                db.Remove(ticket);

                Logger.SendLog($"Deleted ticket n°{ticket.Id}: {ticket.Email}", _userManager.GetUserId(User), db);

                db.SaveChanges();

                return List(ticket.Event.Id);
            }
            catch (Exception e)
            {
                return Details(id, e.Message);
            }
        }

        // POST: User/GenerateKey/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GenerateKey(IFormCollection collection)
        {
            var id = 0;

            try
            {
                using var db = new ServerContext();


                if (collection.TryGetValue("TicketId", out var ticketIdStr))
                {
                    if (int.TryParse(ticketIdStr, out var ticketId))
                        id = ticketId;
                    else return Index("Can't parse ticket id");
                }
                else return Index("Can't find ticket event id input");

                if (!db.Tickets.Any(t => t.Id == id))
                    return Details(id);
                if (db.QrCodes.Any(t => t.Ticket.Id == id))
                    return Details(id);

                var saveableTicket = db.Tickets.Where(t => t.Id == id).Include(t => t.Event).Single();

                if (!DbUtils.IsEventExistingAndUserEventMember(saveableTicket.Event.Id, _userManager.GetUserId(User),
                        db))
                    return Details(id);

                var qrCode = new QrCodeGenerator(saveableTicket);

                var saveableQrCode = qrCode.GenerateKeys();

                saveableQrCode.CreatorId = _userManager.GetUserId(User);

                db.Tickets.Attach(saveableQrCode.Ticket);

                db.QrCodes.Add(saveableQrCode);

                Logger.SendLog($"Generated QR Code for ticket n°{saveableQrCode.Ticket.Id}",
                    _userManager.GetUserId(User), db);

                db.SaveChanges();

                return Details(id);
            }
            catch (Exception)
            {
                return Details(id);
            }
        }


        public ActionResult List(int id)
        {
            return List(id, null);
        }

        private ActionResult List(int id, string error)
        {
            SaveableEvent saveableEvent;
            var list = new List<SaveableTicket>();
            var ticketMailCounts = new Dictionary<int, int>();

            using (var db = new ServerContext())
            {
                if (!DbUtils.IsEventExistingAndUserEventMember(id, _userManager.GetUserId(User), db))
                    return Index("Event is not existing or you're not owner/collaborator of this event");
                saveableEvent = db.Events.Find(id);
                list.AddRange(db.Tickets.Where(t => t.Event.Id == id).ToList());

                db.TicketUserMails.GroupBy(t => t.Ticket.Id).Select(t => new { t.Key, Count = t.Count() }).ToList()
                    .ForEach(t => ticketMailCounts.Add(t.Key, t.Count));
            }

            var model = new TicketListModel(saveableEvent, list, error) { TicketMailCounts = ticketMailCounts };
            return View("List", model);
        }

        public ActionResult Search(int id)
        {
            return View();
        }

        private TicketScanStatsModel GetStats(int id)
        {
            using var db = new ServerContext();
            return new TicketScanStatsModel
            {
                TicketsCount = db.Tickets.Count(t => t.Event.Id == id),
                ScannedTicketsCount = db.Tickets.Count(t =>
                    t.Event.Id == id && db.TicketScans.Any(e => e.Ticket.Id == t.Id)),
                PayedTicketsCount = db.Tickets.Count(t => t.Event.Id == id && t.HasPaid),
                ScannedPayedTicketsCount = db.Tickets.Count(t =>
                    t.Event.Id == id && t.HasPaid && db.TicketScans.Any(e => e.Ticket.Id == t.Id))
            };
        }

        public ActionResult Scan(int id, string error)
        {
            using (var db = new ServerContext())
            {
                if (!DbUtils.IsEventExistingAndUserEventMember(id, _userManager.GetUserId(User), db))
                    return Index("Event is not existing or you're not owner/collaborator of this event");
            }

            return View("Scan", new TicketScanModel(id) { Stats = GetStats(id) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ScanTicket(int id, string input)
        {
            // check if event exists and user is owner/collaborator
            using (var db = new ServerContext())
            {
                if (!DbUtils.IsEventExistingAndUserEventMember(id, _userManager.GetUserId(User), db))
                    return Index("Event is not existing or you're not owner/collaborator of this event");
            }

            if (string.IsNullOrWhiteSpace(input))
                return Scan(id, null);


            using (var db = new ServerContext())
            {
                try
                {
                    var firstDecode = input.Base64Decode();
                    var split = firstDecode.Split("§§");

                    var qrCodeId = int.Parse(split[0]);
                    var qrCode = split[1];

                    if (!db.QrCodes.Any(t => t.Id == qrCodeId))
                        return Scan(id, "QrCode does not exists.");

                    var saveableQrCode = db.QrCodes.Single(t => t.Id == qrCodeId);

                    var qrCodeGenerator = new QrCodeGenerator(saveableQrCode);

                    var decodedString = qrCodeGenerator.Decode(qrCode);

                    var decodedSplit = decodedString.Split("§§");

                    var ticketId = int.Parse(decodedSplit[0]);
                    var ticketEmail = decodedSplit[1];

                    if (!db.Tickets.Any(t => t.Id == ticketId && t.Email.Equals(ticketEmail)))
                        return Scan(id, "Ticket id does not match with email");

                    var ticket = db.Tickets.Single(t => t.Id == ticketId && t.Email.Equals(ticketEmail));

                    var alreadyScanned = db.TicketScans.Any(t => t.Ticket.Id == ticket.Id);

                    var lastScan = null as DateTime?;

                    if (alreadyScanned)
                        lastScan = db.TicketScans.Where(t => t.Ticket.Id == ticket.Id).OrderByDescending(t => t.Date)
                            .Select(t => t.Date).FirstOrDefault();

                    db.TicketScans.Add(new SaveableTicketScan(ticket, _userManager.GetUserId(User), DateTime.UtcNow));
                    db.SaveChanges();

                    return View("Scan",
                        new TicketScanModel(id)
                        {
                            Ticket = ticket, Scanned = true, LastScan = lastScan, Error = null, Stats = GetStats(id)
                        });
                }
                catch (Exception e)
                {
                    return Scan(id, e.Message);
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkAsPaid(int id)
        {
            try
            {
                using var db = new ServerContext();
                if (!DbUtils.IsTicketExisting(id, db))
                    return Details(id, "Ticket does not exists.");
                var ticket = db.Tickets.Include(t => t.Event).Single(t => t.Id == id);

                if (!DbUtils.IsEventExistingAndUserEventMember(ticket.Event.Id, _userManager.GetUserId(User), db))
                    return Details(id, "Event is not existing or you're not owner/collaborator of this event");

                ticket.HasPaid = true;
                ticket.UpdaterId = _userManager.GetUserId(User);
                ticket.UpdatedAt = DateTime.UtcNow;

                db.SaveChanges();

                Logger.SendLog($"Marked ticket n°{ticket.Id} as paid", _userManager.GetUserId(User), db);

                return List(ticket.Event.Id);
            }
            catch (Exception e)
            {
                return Details(id, e.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkScanAsPaid(int id)
        {
            try
            {
                using var db = new ServerContext();
                if (!DbUtils.IsTicketExisting(id, db))
                    return Scan(id, "Ticket does not exists.");
                var ticket = db.Tickets.Include(t => t.Event).Single(t => t.Id == id);

                if (!DbUtils.IsEventExistingAndUserEventMember(ticket.Event.Id, _userManager.GetUserId(User), db))
                    return Scan(id, "Event is not existing or you're not owner/collaborator of this event");

                ticket.HasPaid = true;
                ticket.UpdaterId = _userManager.GetUserId(User);
                ticket.UpdatedAt = DateTime.UtcNow;

                db.SaveChanges();

                Logger.SendLog($"Marked scanned ticket n°{ticket.Id} as paid", _userManager.GetUserId(User), db);

                return View("Scan",
                    new TicketScanModel(ticket.Event.Id)
                        { Ticket = ticket, Scanned = true, Error = null, Stats = GetStats(id) });
            }
            catch (Exception e)
            {
                return Scan(id, e.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendMail(IFormCollection collection)
        {
            var id = 0;

            try
            {
                using var db = new ServerContext();

                if (collection.TryGetValue("TicketId", out var ticketIdStr))
                {
                    if (int.TryParse(ticketIdStr, out var ticketId))
                        id = ticketId;
                    else return Index("Can't parse ticket id");
                }
                else return Index("Can't find ticket event id input");

                if (!db.Tickets.Any(t => t.Id == id))
                    return Details(id);
                //	if (db.QrCodes.Any(t => t.Ticket.Id == id))
                //	return Details(id);

                var saveableTicket = db.Tickets.Where(t => t.Id == id).Include(t => t.Event).Single();

                if (!DbUtils.IsEventExistingAndUserEventMember(saveableTicket.Event.Id, _userManager.GetUserId(User),
                        db))
                    return Details(id);

                var mail = new MailGenerator(saveableTicket, _converter);

                // gen qr code
                SaveableTicketQrCode ticketQrCode;

                if (db.QrCodes.Any(t => t.Ticket.Id == id))
                {
                    ticketQrCode = db.QrCodes.Where(t => t.Ticket.Id == id).Include(t => t.Ticket).Single();
                }
                else
                {
                    var qrCode = new QrCodeGenerator(saveableTicket);

                    var saveableQrCode = qrCode.GenerateKeys();

                    saveableQrCode.CreatorId = _userManager.GetUserId(User);

                    db.Tickets.Attach(saveableQrCode.Ticket);

                    db.QrCodes.Add(saveableQrCode);

                    db.SaveChanges();

                    ticketQrCode = saveableQrCode;
                }

                try
                {
                    mail.SendMailAsync(Environment.GetEnvironmentVariable("SENDGRID_API_KEY"), ticketQrCode)
                        .Wait(TimeSpan.FromSeconds(5));
                }
                catch (OperationCanceledException ex)
                {
                    return Details(id, ex.Message);
                }

                db.TicketUserMails.Add(new SaveableTicketUserMail(saveableTicket, _userManager.GetUserId(User),
                    DateTime.UtcNow));

                Logger.SendLog($"Sent mail for ticket n°{saveableTicket.Id}", _userManager.GetUserId(User), db);

                db.SaveChanges();

                return Details(id);
            }
            catch (Exception e)
            {
                return Details(id, e.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GeneratePdf(IFormCollection collection)
        {
            var id = 0;

            try
            {
                using var db = new ServerContext();

                if (collection.TryGetValue("TicketId", out var ticketIdStr))
                {
                    if (int.TryParse(ticketIdStr, out var ticketId))
                        id = ticketId;
                    else return Index("Can't parse ticket id");
                }
                else return Index("Can't find ticket event id input");

                if (!db.Tickets.Any(t => t.Id == id))
                    return Details(id);
                //	if (db.QrCodes.Any(t => t.Ticket.Id == id))
                //	return Details(id);

                var saveableTicket = db.Tickets.Where(t => t.Id == id).Include(t => t.Event).Single();

                if (!DbUtils.IsEventExistingAndUserEventMember(saveableTicket.Event.Id, _userManager.GetUserId(User),
                        db))
                    return Details(id);

                var mail = new MailGenerator(saveableTicket, _converter);

                // gen qr code
                SaveableTicketQrCode ticketQrCode;

                if (db.QrCodes.Any(t => t.Ticket.Id == id))
                {
                    ticketQrCode = db.QrCodes.Where(t => t.Ticket.Id == id).Include(t => t.Ticket).Single();
                }
                else
                {
                    var qrCode = new QrCodeGenerator(saveableTicket);

                    var saveableQrCode = qrCode.GenerateKeys();

                    saveableQrCode.CreatorId = _userManager.GetUserId(User);

                    db.Tickets.Attach(saveableQrCode.Ticket);

                    db.QrCodes.Add(saveableQrCode);

                    db.SaveChanges();

                    ticketQrCode = saveableQrCode;
                }

                var pdf = new PdfGenerator(ticketQrCode, _converter);

                var pdfBytes = pdf.Generate();

                var fileName = $"Billet0{saveableTicket.Id}_{saveableTicket.FirstName}{saveableTicket.LastName}.pdf";

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception e)
            {
                return Details(id, e.Message);
            }
        }
    }
}