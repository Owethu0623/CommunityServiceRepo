using System.Linq;
using System.Web.Mvc;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.Controllers
{
    public class CitizenComplianceController : Controller
    {
        private Community db = new Community();

        // GET: CitizenCompliance
        public ActionResult Index()
        {
            // Make sure a citizen is logged in
            if (Session["CitizenID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            int citizenId = (int)Session["CitizenID"];

            // Get the logged-in citizen
            Citizen citizen = db.Citizens.Find(citizenId);

            if (citizen == null)
            {
                Session.Clear();

                return RedirectToAction("Index", "Login");
            }

            // Get the citizen's compliance record
            ComplianceRecord complianceRecord = db.ComplianceRecords
                .FirstOrDefault(c => c.CitizenID == citizenId);

            // Pass the citizen to the view
            ViewBag.Citizen = citizen;

            return View(complianceRecord);
        }

        // GET: CitizenCompliance/Details/5
        public ActionResult Details(int? id)
        {
            // Make sure a citizen is logged in
            if (Session["CitizenID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (id == null)
            {
                return RedirectToAction("Index");
            }

            int citizenId = (int)Session["CitizenID"];

            // Get the violation and make sure it belongs
            // to the logged-in citizen's compliance record
            var violation = db.Violations
                .Include("ComplianceRecord")
                .Include("Request")
                .Include("Warnings")
                .FirstOrDefault(v =>
                    v.ViolationID == id.Value &&
                    v.ComplianceRecord.CitizenID == citizenId);

            if (violation == null)
            {
                return HttpNotFound();
            }

            return View(violation);
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