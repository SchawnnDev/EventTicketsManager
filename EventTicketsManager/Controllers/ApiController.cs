using System;
using Library.Api.Json;
using Library.Utils;
using Microsoft.AspNetCore.Mvc;
using Server;
using System.Linq;
using Library.Api;

namespace EventTicketsManager.Controllers
{
    public class ApiController : Controller
    {
        [HttpGet] // format: apiKey§§email
        public ActionResult Login(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return Json(new JsonSuccess(false));

            ApiKeyLogin login;

            try
            {
                login = ParseLogin(id);
            }
            catch (Exception)
            {
                return Json(new JsonSuccess(false));
            }

            using var db = new ServerContext();

            return Json(new JsonSuccess(CheckLogin(login, db)));
        }

        [HttpGet] // format: apiKey§§email$$qrCode
        public ActionResult Scan(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return Json(new JsonSuccess(false));

            ApiScan scan;

            try
            {
                scan = ParseScan(id);
            }
            catch (Exception)
            {
                return Json(new JsonSuccess(false));
            }

            using var db = new ServerContext();

            return CheckScan(scan, db);
        }

        private bool CheckLogin(ApiKeyLogin login, ServerContext db)
        {
            if (!MailUtils.IsEmailValid(login.Email))
                return false;

            if (!db.Events.Any(t => t.ApiKey.Equals(login.ApiKey)) ||
                !db.Users.Any(t => t.Email.ToLower().Equals(login.Email.ToLower())))
                return false;

            var eventId = db.Events.Where(t => t.ApiKey.Equals(login.ApiKey)).Select(t => t.Id).Single();
            var userId = db.Users.Where(t => t.Email.ToLower().Equals(login.Email.ToLower())).Select(t => t.Id)
                .Single();

            return DbUtils.IsEventExistingAndUserEventMember(eventId, userId, db);
        }

        private ActionResult CheckScan(ApiScan scan, ServerContext db)
        {
            if (!CheckLogin(scan.Login, db))
                return Json(new JsonSuccess(false));

            try
            {
                var firstDecode = scan.QrCode.Base64Decode();
                var split = firstDecode.Split("§§");

                var qrCodeId = int.Parse(split[0]);
                var qrCode = split[1];

                if (!db.QrCodes.Any(t => t.Id == qrCodeId))
                    return Json(new JsonSuccess(false));

                var saveableQrCode = db.QrCodes.Single(t => t.Id == qrCodeId);

                var qrCodeGenerator = new QrCodeGenerator(saveableQrCode);

                var decodedString = qrCodeGenerator.Decode(qrCode);

                var decodedSplit = decodedString.Split("§§");

                var ticketId = int.Parse(decodedSplit[0]);
                var ticketEmail = decodedSplit[1];

                if(!db.Tickets.Any(t=>t.Id == ticketId && t.Email.Equals(ticketEmail)))
                    return Json(new JsonSuccess(false));

                var ticket = db.Tickets.Single(t => t.Id == ticketId && t.Email.Equals(ticketEmail));

                var alreadyScanned = db.TicketScans.Any(t => t.Ticket.Id == ticket.Id);

                return Json(new JsonScan(ticket.FirstName, ticket.LastName, ticket.HasPaid, alreadyScanned, ticket.ToPay));

            }
            catch (Exception e)
            {
                return Json(new JsonSuccess(false){Error = e.Message});
            }
        }

        private ActionResult EmptyResult() => Json("[]");

        private ApiKeyLogin ParseLogin(string text)
        {
            var decodedText = text.Base64Decode();
            var split = decodedText.Split("§§");
            return new ApiKeyLogin(split[0], split[1]);
        }

        private ApiScan ParseScan(string text)
        {
            var decodedText = text.Base64Decode();
            var split = decodedText.Split("§§");
            return new ApiScan(split[0], split[1], split[2]);
        }
    }

    internal class ApiScan
    {
        public ApiKeyLogin Login { get; set; }

        public string QrCode { get; set; }

        public ApiScan(string apiKey, string email, string qrCode) : this(new ApiKeyLogin(apiKey, email), qrCode)
        {
        }

        public ApiScan(ApiKeyLogin login, string qrCode)
        {
            Login = login;
            QrCode = qrCode;
        }
    }

    internal class ApiKeyLogin
    {
        public string ApiKey { get; set; }
        public string Email { get; set; }

        public ApiKeyLogin(string apiKey, string email)
        {
            ApiKey = apiKey;
            Email = email;
        }
    }
}