using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventTicketsManager.Models;
using Library.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server;

namespace EventTicketsManager.Controllers
{
    [Authorize]
    public class TicketController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public TicketController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        // GET: User
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


            return View("Index", new TicketIndexModel(events));
        }

        // GET: User/Details/5
        public ActionResult Details(int id)
        {
            SaveableTicket model = null;

            using (var db = new ServerContext())
            {
                if (DbUtils.IsTicketExisting(id,db))
                    model = db.Tickets.Find(id);
            }

            return model != null ? View(model) : View("Index");
        }

        // GET: User/Create
        public ActionResult Create(int id)
        {
            decimal toPay;
            using (var db = new ServerContext())
            {
                if (!DbUtils.IsEventExisting(id, db)) return View("Index");
                toPay = db.Events.Where(t => t.Id == id).Select(t => t.EnterPrice).Single();
            }

            return View(new SaveableTicket {TicketEventId = id, ToPay = toPay});
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
                    else return Index();
                } else return Index();

                var saveableTicket = new SaveableTicket();

                using (var db = new ServerContext())
                {

                    if (!DbUtils.IsEventExistingAndUserEventMember(eventId, _userManager.GetUserId(User), db)) return List(eventId);

                    FillTicket(saveableTicket, collection);

                    var saveableEvent = db.Events.Find(eventId);

                    saveableTicket.Event = saveableEvent;
                    saveableTicket.CreatorId = saveableTicket.UpdaterId = _userManager.GetUserId(User);
                    saveableTicket.CreatedAt = saveableTicket.UpdatedAt = DateTime.Now;

                    db.Tickets.Add(saveableTicket);
                    db.SaveChanges();
                }


                return List(eventId);
            }
            catch
            {
                return Index();
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

            return model != null ? View(model) : View("Index");
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

                    if (!DbUtils.IsTicketExisting(id, db)) return Index();

                    var saveableTicket = db.Tickets.Include(t=>t.Event).Single(t => t.Id==id);

                    eventId = saveableTicket.Event.Id;

                    if (!DbUtils.IsEventExistingAndUserEventMember(eventId, _userManager.GetUserId(User), db))
                        return List(eventId);

                    FillTicket(saveableTicket, collection);

                    saveableTicket.UpdaterId = _userManager.GetUserId(User);
                    saveableTicket.UpdatedAt = DateTime.Now;

                    db.Events.Attach(saveableTicket.Event);
                    db.SaveChanges();
                }


                return List(eventId);
            }
            catch (Exception e)
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
            if (collection.TryGetValue("Email", out var email))
                saveableTicket.Email = email;
            if (collection.TryGetValue("Gender", out var genderStr))
                if(int.TryParse(genderStr, out var gender))
                    saveableTicket.Gender = gender;
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

        public ActionResult List(int id)
        {
            var list = new List<SaveableTicket>();

            using (var db = new ServerContext())
                list.AddRange(db.Tickets.Where(t => t.Event.Id == id).ToList());

            return View("List", list);
        }

        public ActionResult Search(int id)
        {
            return View();
        }
    }
}