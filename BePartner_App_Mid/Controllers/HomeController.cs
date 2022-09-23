using BePartner_App_Mid.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BePartner_App_Mid.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var db = new bePartnerCentralDatabaseEntities2();
            var data = (from I in db.Ideas where I.Status.Equals("Confirmed") select I).ToList();
            return View(data);
            
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Blog()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}