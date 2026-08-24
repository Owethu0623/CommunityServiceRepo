using System.Linq;
using System.Web.Mvc;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.Controllers
{
    public class LoginController : Controller
    {
        private Community db = new Community();


        // =========================================================
        // GET: Login
        // =========================================================

        public ActionResult Index()
        {
            return View();
        }


        // =========================================================
        // POST: Login
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Find the citizen using email and password
                var citizen = db.Citizens.FirstOrDefault(c =>
                    c.EmailAddress == model.EmailAddress &&
                    c.Password == model.Password
                );


                // =================================================
                // Citizen does not exist / incorrect credentials
                // =================================================

                if (citizen == null)
                {
                    ModelState.AddModelError(
                        "",
                        "Invalid email or password."
                    );

                    return View(model);
                }


                // =================================================
                // Account suspended
                // =================================================

                if (citizen.AccountStatus == AccountStatus.Suspended)
                {
                    ModelState.AddModelError(
                        "",
                        "Your account has been suspended. Please contact the municipality."
                    );

                    return View(model);
                }


                // =================================================
                // Account inactive
                // =================================================

                if (citizen.AccountStatus == AccountStatus.Inactive)
                {
                    ModelState.AddModelError(
                        "",
                        "Your account is inactive. Please contact the municipality."
                    );

                    return View(model);
                }


                // =================================================
                // Login successful
                // =================================================

                Session["CitizenID"] =
                    citizen.CitizenID;

                Session["CitizenName"] =
                    citizen.FirstName;

                Session["CitizenEmail"] =
                    citizen.EmailAddress;


                return RedirectToAction(
                    "Index",
                    "CitizenDashboard"
                );
            }


            return View(model);
        }


        // =========================================================
        // GET: Logout
        // =========================================================

        public ActionResult Logout()
        {
            // Remove the citizen's login session
            Session.Remove("CitizenID");
            Session.Remove("CitizenName");
            Session.Remove("CitizenEmail");

            // Return to the public Home page
            return RedirectToAction(
                "Index",
                "Home"
            );
        }


        // =========================================================
        // DISPOSE
        // =========================================================

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