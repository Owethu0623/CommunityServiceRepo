using System;
using System.Linq;
using System.Web.Mvc;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.Controllers
{
    public class WardManagementController : Controller
    {
        private Community db = new Community();


        // =========================================================
        // WARD MANAGEMENT
        // =========================================================

        public ActionResult Index(string search)
        {
            // Make sure administrator is logged in
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            var wards = db.Wards.AsQueryable();

            // Search by ward name or ward number
            if (!string.IsNullOrWhiteSpace(search))
            {
                wards = wards.Where(w =>
                    w.WardName.Contains(search) ||
                    w.WardNumber.Contains(search)
                );
            }

            // Convert WardNumber to an integer for proper numeric ordering
            var wardList = wards
                .ToList()
                .OrderBy(w =>
                {
                    int wardNumber;

                    return int.TryParse(w.WardNumber, out wardNumber)
                        ? wardNumber
                        : int.MaxValue;
                })
                .ToList();

            ViewBag.Search = search;

            return View(wardList);
        }


        // =========================================================
        // GET: ADD WARD
        // =========================================================

        public ActionResult Create()
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            return View();
        }


        // =========================================================
        // POST: ADD WARD
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Ward ward)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            if (ModelState.IsValid)
            {
                // Prevent duplicate ward numbers
                bool wardNumberExists = db.Wards.Any(w =>
                    w.WardNumber == ward.WardNumber
                );

                if (wardNumberExists)
                {
                    ModelState.AddModelError(
                        "WardNumber",
                        "This ward number already exists."
                    );

                    return View(ward);
                }

                // New wards are active by default
                ward.IsActive = true;

                db.Wards.Add(ward);
                db.SaveChanges();

                TempData["SuccessMessage"] =
                    "Ward added successfully.";

                return RedirectToAction("Index");
            }

            return View(ward);
        }


        // =========================================================
        // GET: EDIT WARD
        // =========================================================

        public ActionResult Edit(int id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            var ward = db.Wards.Find(id);

            if (ward == null)
            {
                return HttpNotFound();
            }

            return View(ward);
        }


        // =========================================================
        // POST: EDIT WARD
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Ward ward)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            if (ModelState.IsValid)
            {
                bool duplicateWardNumber = db.Wards.Any(w =>
                    w.WardNumber == ward.WardNumber &&
                    w.WardID != ward.WardID
                );

                if (duplicateWardNumber)
                {
                    ModelState.AddModelError(
                        "WardNumber",
                        "This ward number already exists."
                    );

                    return View(ward);
                }

                var existingWard = db.Wards.Find(ward.WardID);

                if (existingWard == null)
                {
                    return HttpNotFound();
                }

                existingWard.WardName =
                    ward.WardName;

                existingWard.WardNumber =
                    ward.WardNumber;

                existingWard.Description =
                    ward.Description;

                db.SaveChanges();

                TempData["SuccessMessage"] =
                    "Ward updated successfully.";

                return RedirectToAction("Index");
            }

            return View(ward);
        }


        // =========================================================
        // DEACTIVATE WARD
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deactivate(int id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            var ward = db.Wards.Find(id);

            if (ward == null)
            {
                return HttpNotFound();
            }

            ward.IsActive = false;

            db.SaveChanges();

            TempData["SuccessMessage"] =
                "Ward deactivated successfully.";

            return RedirectToAction("Index");
        }


        // =========================================================
        // ACTIVATE WARD
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Activate(int id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            var ward = db.Wards.Find(id);

            if (ward == null)
            {
                return HttpNotFound();
            }

            ward.IsActive = true;

            db.SaveChanges();

            TempData["SuccessMessage"] =
                "Ward activated successfully.";

            return RedirectToAction("Index");
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