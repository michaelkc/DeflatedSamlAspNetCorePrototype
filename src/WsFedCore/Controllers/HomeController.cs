using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WsFedCore.DeflatedSamlBearerAuthentication;
using WsFedCore.Models;

namespace WsFedCore.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(AuthenticationSchemes = DeflatedSamlBearerDefaults.AuthenticationScheme)]
        public IActionResult Api()
        {
            return Content(this.ControllerContext.HttpContext.User.Identity.Name);
        }


        [Authorize]
        public IActionResult About()
        {
            ViewData["Message"] = this.ControllerContext.HttpContext.User.Identity.Name;

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
