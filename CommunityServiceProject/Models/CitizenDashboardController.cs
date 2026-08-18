using System.Net;
using System.Web.Mvc;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.Controllers
{
    public class CitizenDashboardController : Controller
    {
        private Community db = new Community();

        // GET: CitizenDashboard
        public ActionResult Index()
        {
            // Make sure a citizen is actually logged in
            if (Session["CitizenID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            int citizenId = (int)Session["CitizenID"];

            // Get the actual logged-in citizen
            Citizen citizen = db.Citizens.Find(citizenId);

            if (citizen == null)
            {
                Session.Clear();
                return RedirectToAction("Index", "Login");
            }

            return View(citizen);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}