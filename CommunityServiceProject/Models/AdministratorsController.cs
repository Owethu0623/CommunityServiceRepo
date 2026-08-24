using System.Linq;
using System.Web.Mvc;

namespace CommunityServiceProject.Models
{
    public class AdministratorsController : Controller
    {
        private Community db = new Community();

        // GET: Administrators/Login
        public ActionResult Login()
        {
            return View();
        }

        // GET: Administrators/Logout
        public ActionResult Logout()
        {
            Session.Clear();

            return RedirectToAction("Login", "Administrators");
        }

        // POST: Administrators/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string emailAddress, string password)
        {
            var administrator = db.Administrators.FirstOrDefault(a =>
                a.EmailAddress == emailAddress &&
                a.Password == password &&
                a.AccountStatus == AccountStatus.Active
            );

            if (administrator != null)
            {
                Session["AdministratorID"] = administrator.AdministratorID;
                Session["AdministratorName"] = administrator.FirstName;

                return RedirectToAction("Index", "AdministratorDashboard");
            }

            ViewBag.Error = "Invalid email address or password.";
            return View();
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