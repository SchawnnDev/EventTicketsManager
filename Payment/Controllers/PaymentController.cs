using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Payment.Controllers
{
    public class PaymentController : Controller
    {
        public IActionResult Index()
        {
            return Redirect("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=8788DA34DTPNL&source=url");
        }

        public IActionResult Success()
        {
            return View();
        }

        public IActionResult Cancel()
        {
            return Content("Tu as annulé le paiment.");
        }

    }
}