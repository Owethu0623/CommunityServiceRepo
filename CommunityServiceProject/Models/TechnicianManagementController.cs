using System.Linq;
using System.Web.Mvc;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.Controllers
{
    public class TechnicianManagementController : Controller
    {
        private Community db = new Community();

        // GET: TechnicianManagement
        public ActionResult Index()
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            var technicians = db.Technicians
                .OrderBy(t => t.LastName)
                .ThenBy(t => t.FirstName)
                .ToList();

            return View(technicians);
        }

        // GET: TechnicianManagement/Details/5
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

            var technician = db.Technicians
                .FirstOrDefault(t => t.TechnicianID == id);

            if (technician == null)
            {
                return HttpNotFound();
            }

            return View(technician);
        }

        // GET: TechnicianManagement/Create
        public ActionResult Create()
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            return View();
        }


        // POST: TechnicianManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Technician technician)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            if (ModelState.IsValid)
            {
                technician.AccountStatus = AccountStatus.Active;

                db.Technicians.Add(technician);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(technician);
        }

        // POST: TechnicianManagement/Activate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Activate(int id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            var technician = db.Technicians.Find(id);

            if (technician == null)
            {
                return HttpNotFound();
            }

            technician.AccountStatus = AccountStatus.Active;

            db.SaveChanges();

            return RedirectToAction("Details", new { id = id });
        }


        // POST: TechnicianManagement/Suspend
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Suspend(int id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            var technician = db.Technicians.Find(id);

            if (technician == null)
            {
                return HttpNotFound();
            }

            technician.AccountStatus = AccountStatus.Suspended;

            db.SaveChanges();

            return RedirectToAction("Details", new { id = id });
        }


        // POST: TechnicianManagement/Deactivate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deactivate(int id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            var technician = db.Technicians.Find(id);

            if (technician == null)
            {
                return HttpNotFound();
            }

            technician.AccountStatus = AccountStatus.Inactive;

            db.SaveChanges();

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