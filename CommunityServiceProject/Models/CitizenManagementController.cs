using System.Linq;
using System.Web.Mvc;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.Controllers
{
    public class CitizenManagementController : Controller
    {
        private Community db = new Community();

        // GET: CitizenManagement
        public ActionResult Index()
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            var citizens = db.Citizens
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .ToList();

            return View(citizens);
        }

        // GET: CitizenManagement/Details/5
        public ActionResult Details(int? id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            if (id == null)
            {
                return RedirectToAction("Index");
            }

            var citizen = db.Citizens
                .FirstOrDefault(c => c.CitizenID == id);

            if (citizen == null)
            {
                return HttpNotFound();
            }

            return View(citizen);
        }

        // POST: CitizenManagement/Activate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Activate(int id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            var citizen = db.Citizens.Find(id);

            if (citizen == null)
            {
                return HttpNotFound();
            }

            db.Database.ExecuteSqlCommand(
                "UPDATE Citizens SET AccountStatus = @p0 WHERE CitizenID = @p1",
                (int)AccountStatus.Active,
                id
            );

            return RedirectToAction("Details", new { id = id });
        }


        // POST: CitizenManagement/Suspend
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Suspend(int id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            var citizen = db.Citizens.Find(id);

            if (citizen == null)
            {
                return HttpNotFound();
            }

            db.Database.ExecuteSqlCommand(
                "UPDATE Citizens SET AccountStatus = @p0 WHERE CitizenID = @p1",
                (int)AccountStatus.Suspended,
                id
            );

            return RedirectToAction("Details", new { id = id });
        }

        // POST: CitizenManagement/Deactivate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deactivate(int id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            var citizen = db.Citizens.Find(id);

            if (citizen == null)
            {
                return HttpNotFound();
            }

            db.Database.ExecuteSqlCommand(
                "UPDATE Citizens SET AccountStatus = @p0 WHERE CitizenID = @p1",
                (int)AccountStatus.Inactive,
                id
            );

            return RedirectToAction("Details", new { id = id });
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