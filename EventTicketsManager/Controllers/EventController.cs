using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
                events.AddRange(db.Events.Where(t=>t.Creator.Id == userId).ToList());

                var list = db.EventUsers.Where(t => t.User.Id == userId)
                    .Select(t => t.Event).ToList();

                foreach (var saveableEvent in list)
                {
	                saveableEvent.IsCreator = false;

					if (events.All(t => t.Id != saveableEvent.Id))
						events.Add(saveableEvent);
	               
				}

            }

            return View(events);
        }

        // GET: Event/Details/5
        public ActionResult Details(int id)
        {

	        SaveableEvent model = null;

			using(var db = new ServerContext())
			{
				if (db.Events.Any(t => t.Id == id))
					model = db.Events.Find(id);
			}

			return model != null ? View(model) : View("Index");
        }

        // GET: Event/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Event/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(IFormCollection collection)
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

				saveableEvent.Creator = await _userManager.GetUserAsync(User);

				await using (var db = new ServerContext())
				{
					db.Users.Attach(saveableEvent.Creator);
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

			using(var db = new ServerContext())
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
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Event/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
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
    }
}