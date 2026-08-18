using System.Linq;
using System.Web.Mvc;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.Controllers
{
    public class ForgotPasswordController : Controller
    {
        private Community db = new Community();

        // GET: ForgotPassword
        public ActionResult Index()
        {
            return View(new ForgotPasswordViewModel());
        }

        // POST: ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(ForgotPasswordViewModel model, string action)
        {
            // Verify email
            if (action == "VerifyEmail")
            {
                if (string.IsNullOrEmpty(model.EmailAddress))
                {
                    ModelState.AddModelError(
                        "EmailAddress",
                        "Please enter your email address."
                    );

                    return View(model);
                }

                var citizen = db.Citizens.FirstOrDefault(c =>
                    c.EmailAddress == model.EmailAddress);

                if (citizen == null)
                {
                    ModelState.AddModelError(
                        "EmailAddress",
                        "No account was found with this email address."
                    );

                    return View(model);
                }

                model.EmailVerified = true;
                ModelState.Clear();

                return View(model);
            }

            // Reset password
            if (action == "ResetPassword")
            {
                var citizen = db.Citizens.FirstOrDefault(c =>
                    c.EmailAddress == model.EmailAddress);

                if (citizen == null)
                {
                    ModelState.AddModelError(
                        "EmailAddress",
                        "Email address could not be verified."
                    );

                    return View(model);
                }

                if (string.IsNullOrEmpty(model.NewPassword))
                {
                    ModelState.AddModelError(
                        "NewPassword",
                        "Please enter a new password."
                    );

                    model.EmailVerified = true;

                    return View(model);
                }

                if (model.NewPassword != model.ConfirmPassword)
                {
                    ModelState.AddModelError(
                        "ConfirmPassword",
                        "Passwords do not match."
                    );

                    model.EmailVerified = true;

                    return View(model);
                }

                citizen.Password = model.NewPassword;
                citizen.ConfirmPassword = model.ConfirmPassword;

                db.SaveChanges();

                db.SaveChanges();

                TempData["Message"] =
                    "Your password has been successfully reset.";

                return RedirectToAction("Index", "Login");
            }

            return View(model);
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