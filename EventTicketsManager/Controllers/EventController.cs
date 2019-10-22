using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using EventTicketsManager.Models;
using Library.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Server;

namespace EventTicketsManager.Controllers
{
    [Authorize]
    public class EventController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public EventController(UserManager<IdentityUser> userManager)
        {
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
        public ActionResult Details(int id)
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
            }

            return model.Event != null ? View("Details", model) : View("Index");
        }

        // GET: Event/Create
        public ActionResult Create()
        {
            return View(new SaveableEvent{Start = DateTime.Now, End = DateTime.Now.AddDays(1)});
        }

        // POST: Event/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                var saveableEvent = new SaveableEvent();

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
                    saveableEvent.EnterPrice = decimal.Parse(enterPrice);
                if (collection.TryGetValue("Start", out var start))
                    saveableEvent.Start = DateTime.Parse(start);
                if (collection.TryGetValue("End", out var end))
                    saveableEvent.End = DateTime.Parse(end);
                if (collection.TryGetValue("Enabled", out var enabled))
                    saveableEvent.Enabled = bool.Parse(enabled.ToString().Split(",")[0]);

                saveableEvent.CreatorId = _userManager.GetUserId(User);
                saveableEvent.CreatedAt = DateTime.Now;
                saveableEvent.UpdatedAt = DateTime.Now;
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
                        saveableEvent.EnterPrice = decimal.Parse(enterPrice);
                    if (collection.TryGetValue("Start", out var start))
                        saveableEvent.Start = DateTime.Parse(start);
                    if (collection.TryGetValue("End", out var end))
                        saveableEvent.End = DateTime.Parse(end);
                    if (collection.TryGetValue("Enabled", out var enabled))
                        saveableEvent.Enabled = bool.Parse(enabled.ToString().Split(",")[0]);

                    saveableEvent.UpdatedAt = DateTime.Now;

                    db.SaveChanges();
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                return View("Index");
            }
        }

        // GET: Event/Delete/5
        public ActionResult Delete(int id)
        {
            /*
            using var db = new ServerContext();
            return View(db.Events.Single(t => t.Id == id)); */

            return Details(id);
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
            catch (Exception e)
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
            catch (Exception e)
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
            catch (Exception e)
            {
                return Details(eventId);
            }
        }
    }
}