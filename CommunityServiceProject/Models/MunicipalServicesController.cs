
using CommunityServiceProject.Models;
using System.Linq;
using System.Web.Mvc;

namespace CommunityServiceProject.Controllers
{
    public class MunicipalServicesController : Controller
    {
        private readonly Community db = new Community();

        [HttpGet]
        public ActionResult Index()
        {
            if (Session["CitizenID"] == null)
                return RedirectToAction("Login", "Login");

            var services = db.ServiceTypes
                .Where(s => s.IsActive)
                .OrderBy(s => s.ServiceName)
                .ToList();

            return View(services);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}

