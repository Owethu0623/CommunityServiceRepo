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
                // Login successful
                // =================================================

                if (citizen != null)
                {
                    // Store logged-in citizen information
                    Session["CitizenID"] = citizen.CitizenID;

                    Session["CitizenName"] = citizen.FirstName;

                    Session["CitizenEmail"] = citizen.EmailAddress;


                    // Send citizen to dashboard
                    return RedirectToAction(
                        "Index",
                        "CitizenDashboard"
                    );
                }


                // =================================================
                // Login failed
                // =================================================

                ModelState.AddModelError(
                    "",
                    "Invalid email or password."
                );
            }


            return View(model);
        }


        // =========================================================
        // LOGOUT
        // =========================================================

        public ActionResult Logout()
        {
            // Clear the logged-in citizen information
            Session.Clear();

            Session.Abandon();


            // Return to Login
            return RedirectToAction(
                "Index",
                "Login"
            );
        }


        // =========================================================
        // Dispose
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