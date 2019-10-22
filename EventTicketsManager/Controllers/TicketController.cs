using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventTicketsManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
            return View();
        }

        // GET: User/Create
        public ActionResult Create(int id)
        {
            return View(new SaveableTicket {TicketEventId = id});
        }

        // POST: User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: User/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
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

            return View(list);
        }

        public ActionResult Search(int id)
        {
            return View();
        }
    }
}