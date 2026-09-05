using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace CommunityServiceProject.Models
{
    public class CitizensController : Controller
    {
        private Community db = new Community();


        // =========================================================
        // GET: Citizens
        // =========================================================

        public ActionResult Index()
        {
            return View(db.Citizens.ToList());
        }


        // =========================================================
        // GET: Citizens/Details/5
        // =========================================================

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest
                );
            }

            Citizen citizen = db.Citizens.Find(id);

            if (citizen == null)
            {
                return HttpNotFound();
            }

            return View(citizen);
        }


        // =========================================================
        // GET: Citizens/Create
        // =========================================================

        public ActionResult Create()
        {
            return View();
        }


        // POST: Citizens/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "FirstName,LastName,EmailAddress,PhoneNumber,Password,ConfirmPassword,ResidentialAddress")]
    Citizen citizen)
        {
            // Clean up the email before checking/saving it
            if (!string.IsNullOrWhiteSpace(citizen.EmailAddress))
            {
                citizen.EmailAddress = citizen.EmailAddress.Trim();
            }

            // Check whether the email already belongs to another account
            if (!string.IsNullOrWhiteSpace(citizen.EmailAddress))
            {
                bool emailExists = db.Citizens
                    .Any(c => c.EmailAddress.ToLower() == citizen.EmailAddress.ToLower());

                if (emailExists)
                {
                    ModelState.AddModelError(
                        "EmailAddress",
                        "An account with this email address already exists."
                    );
                }
            }

            // Only create the account if ALL validation passes
            if (ModelState.IsValid)
            {
                citizen.DateRegistered = DateTime.Now;
                citizen.AccountStatus = AccountStatus.Active;

                db.Citizens.Add(citizen);
                db.SaveChanges();

                return RedirectToAction("Index", "Login");
            }

            return View(citizen);
        
        }


        // =========================================================
        // GET: Citizens/Edit/5
        // =========================================================

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest
                );
            }

            Citizen citizen = db.Citizens.Find(id);

            if (citizen == null)
            {
                return HttpNotFound();
            }

            return View(citizen);
        }


        // =========================================================
        // POST: Citizens/Edit/5
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include =
                "CitizenID,FirstName,LastName,EmailAddress,PhoneNumber,Password,ResidentialAddress,DateRegistered,AccountStatus")]
            Citizen citizen)
        {
            if (ModelState.IsValid)
            {
                db.Entry(citizen).State = EntityState.Modified;

                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(citizen);
        }


        // =========================================================
        // GET: Citizens/UpdateProfile
        // =========================================================

        public ActionResult UpdateProfile()
        {
            if (Session["CitizenID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            int citizenId = (int)Session["CitizenID"];

            Citizen citizen = db.Citizens.Find(citizenId);

            if (citizen == null)
            {
                Session.Clear();
                return RedirectToAction("Index", "Login");
            }

            CitizenProfileViewModel model = new CitizenProfileViewModel
            {
                FirstName = citizen.FirstName,
                LastName = citizen.LastName,
                EmailAddress = citizen.EmailAddress,
                PhoneNumber = citizen.PhoneNumber,
                ResidentialAddress = citizen.ResidentialAddress
            };

            return View(model);
        }


        // =========================================================
        // POST: Citizens/UpdateProfile
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(CitizenProfileViewModel updatedCitizen)
        {
            if (Session["CitizenID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            int citizenId = (int)Session["CitizenID"];

            Citizen citizen = db.Citizens.Find(citizenId);

            if (citizen == null)
            {
                Session.Clear();
                return RedirectToAction("Index", "Login");
            }

            // Check profile validation
            if (!ModelState.IsValid)
            {
                return View(updatedCitizen);
            }

            // Update profile fields ONLY
            citizen.FirstName = updatedCitizen.FirstName;
            citizen.LastName = updatedCitizen.LastName;
            citizen.EmailAddress = updatedCitizen.EmailAddress;
            citizen.PhoneNumber = updatedCitizen.PhoneNumber;
            citizen.ResidentialAddress = updatedCitizen.ResidentialAddress;

            // IMPORTANT:
            // Citizen requires Password and ConfirmPassword.
            // Profile editing does not change the password.
            // Keep the existing password and make ConfirmPassword
            // match it for entity validation.
            citizen.ConfirmPassword = citizen.Password;

            try
            {
                db.SaveChanges();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                foreach (var entityValidationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in entityValidationErrors.ValidationErrors)
                    {
                        ModelState.AddModelError(
                            validationError.PropertyName,
                            validationError.ErrorMessage
                        );
                    }
                }

                return View(updatedCitizen);
            }

            // =====================================================
            // Store FIRST NAME ONLY for the dashboard
            // =====================================================

            Session["CitizenName"] = citizen.FirstName;

            // =====================================================
            // Go back to Citizen Dashboard
            // =====================================================

            return RedirectToAction(
                "Index",
                "CitizenDashboard"
            );
        }


        // =========================================================
        // GET: Citizens/Delete/5
        // =========================================================

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest
                );
            }

            Citizen citizen = db.Citizens.Find(id);

            if (citizen == null)
            {
                return HttpNotFound();
            }

            return View(citizen);
        }


        // =========================================================
        // POST: Citizens/Delete/5
        // =========================================================

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Citizen citizen = db.Citizens.Find(id);

            db.Citizens.Remove(citizen);

            db.SaveChanges();

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
