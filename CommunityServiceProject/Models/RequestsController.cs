using System;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.Controllers
{
    public class RequestsController : Controller
    {
        private Community db = new Community();


        // =========================================================
        // GET: Requests
        // =========================================================

        public ActionResult Index()
        {
            // Get the currently logged-in citizen
            if (Session["CitizenID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            int citizenId = (int)Session["CitizenID"];


            var requests = db.Requests
                .Include(r => r.Category)
                .Include(r => r.Citizen)
                .Where(r => r.CitizenID == citizenId)
                .OrderByDescending(r => r.DateSubmitted)
                .ToList();

            return View(requests);
        }


        // =========================================================
        // GET: Requests/Details/5
        // =========================================================

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest
                );
            }


            if (Session["CitizenID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }


            int citizenId = (int)Session["CitizenID"];


            Request request = db.Requests
                .Include(r => r.Category)
                .Include(r => r.Citizen)
                .FirstOrDefault(
                    r =>
                        r.RequestID == id &&
                        r.CitizenID == citizenId
                );


            if (request == null)
            {
                return HttpNotFound();
            }


            return View(request);
        }


        // =========================================================
        // GET: Requests/Track/5
        // =========================================================

        public ActionResult Track(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest
                );
            }


            if (Session["CitizenID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }


            int citizenId = (int)Session["CitizenID"];


            Request request = db.Requests
                .Include(r => r.Category)
                .Include(r => r.Citizen)
                .FirstOrDefault(
                    r =>
                        r.RequestID == id &&
                        r.CitizenID == citizenId
                );


            if (request == null)
            {
                return HttpNotFound();
            }


            return View(request);
        }


        // =========================================================
        // GET: Requests/Create
        // =========================================================

        public ActionResult Create()
        {
            if (Session["CitizenID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }


            ViewBag.CategoryID = new SelectList(
                db.Categories,
                "CategoryID",
                "CategoryName"
            );


            return View();
        }


        // =========================================================
        // POST: Requests/Create
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include =
                "Title,Description,LocationDescription,CategoryID")]
            Request request,
            string Latitude,
            string Longitude,
            HttpPostedFileBase ImageFile)
        {
            if (Session["CitizenID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }


            // ---------------------------------------------------------
            // Validate Latitude
            // ---------------------------------------------------------

            if (string.IsNullOrWhiteSpace(Latitude))
            {
                ModelState.AddModelError(
                    "Latitude",
                    "Please select the problem location on the map."
                );
            }
            else
            {
                double latitudeValue;

                if (double.TryParse(
                    Latitude,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out latitudeValue))
                {
                    if (latitudeValue < -90 ||
                        latitudeValue > 90)
                    {
                        ModelState.AddModelError(
                            "Latitude",
                            "The selected latitude is not valid."
                        );
                    }
                    else
                    {
                        request.Latitude = latitudeValue;
                    }
                }
                else
                {
                    ModelState.AddModelError(
                        "Latitude",
                        "The latitude value is not valid."
                    );
                }
            }


            // ---------------------------------------------------------
            // Validate Longitude
            // ---------------------------------------------------------

            if (string.IsNullOrWhiteSpace(Longitude))
            {
                ModelState.AddModelError(
                    "Longitude",
                    "Please select the problem location on the map."
                );
            }
            else
            {
                double longitudeValue;

                if (double.TryParse(
                    Longitude,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out longitudeValue))
                {
                    if (longitudeValue < -180 ||
                        longitudeValue > 180)
                    {
                        ModelState.AddModelError(
                            "Longitude",
                            "The selected longitude is not valid."
                        );
                    }
                    else
                    {
                        request.Longitude = longitudeValue;
                    }
                }
                else
                {
                    ModelState.AddModelError(
                        "Longitude",
                        "The longitude value is not valid."
                    );
                }
            }


            // ---------------------------------------------------------
            // Save Request
            // ---------------------------------------------------------

            if (ModelState.IsValid)
            {
                // System-managed information
                request.DateSubmitted = DateTime.Now;

                request.Status =
                    RequestStatus.Pending;


                // Logged-in citizen
                request.CitizenID =
                    (int)Session["CitizenID"];


                // No administrator or technician yet
                request.AdministratorID = null;

                request.TechnicianID = null;


                // -----------------------------------------------------
                // Handle uploaded image
                // -----------------------------------------------------

                if (ImageFile != null &&
                    ImageFile.ContentLength > 0)
                {
                    string uploadFolder =
                        Server.MapPath("~/Uploads/");


                    if (!System.IO.Directory.Exists(
                        uploadFolder))
                    {
                        System.IO.Directory.CreateDirectory(
                            uploadFolder
                        );
                    }


                    string fileName =
                        System.IO.Path.GetFileName(
                            ImageFile.FileName
                        );


                    string path =
                        System.IO.Path.Combine(
                            uploadFolder,
                            fileName
                        );


                    ImageFile.SaveAs(path);


                    request.ImagePath =
                        "~/Uploads/" + fileName;
                }


                // -----------------------------------------------------
                // Save to database
                // -----------------------------------------------------

                db.Requests.Add(request);

                db.SaveChanges();


                return RedirectToAction("Index");
            }


            // ---------------------------------------------------------
            // Validation failed
            // ---------------------------------------------------------

            ViewBag.CategoryID = new SelectList(
                db.Categories,
                "CategoryID",
                "CategoryName",
                request.CategoryID
            );


            return View(request);
        }


        // =========================================================
        // GET: Requests/Edit/5
        // =========================================================

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest
                );
            }


            if (Session["CitizenID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }


            int citizenId =
                (int)Session["CitizenID"];


            // Only find requests belonging to this citizen
            Request request = db.Requests
                .FirstOrDefault(
                    r =>
                        r.RequestID == id &&
                        r.CitizenID == citizenId
                );


            if (request == null)
            {
                return HttpNotFound();
            }


            // -----------------------------------------------------
            // Only Pending requests can be edited
            // -----------------------------------------------------

            if (!request.CanEdit())
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest,
                    "This request can no longer be edited."
                );
            }


            // -----------------------------------------------------
            // Category dropdown
            // -----------------------------------------------------

            ViewBag.CategoryID = new SelectList(
                db.Categories,
                "CategoryID",
                "CategoryName",
                request.CategoryID
            );


            return View(request);
        }


        // =========================================================
        // POST: Requests/Edit/5
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include =
                "RequestID,Title,Description,LocationDescription,CategoryID")]
            Request updatedRequest,
            HttpPostedFileBase ImageFile)
        {
            if (Session["CitizenID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }


            int citizenId =
                (int)Session["CitizenID"];


            // -----------------------------------------------------
            // Validation
            // -----------------------------------------------------

            if (!ModelState.IsValid)
            {
                ViewBag.CategoryID = new SelectList(
                    db.Categories,
                    "CategoryID",
                    "CategoryName",
                    updatedRequest.CategoryID
                );


                return View(updatedRequest);
            }


            // -----------------------------------------------------
            // Find original request
            // -----------------------------------------------------

            Request request = db.Requests
                .FirstOrDefault(
                    r =>
                        r.RequestID ==
                        updatedRequest.RequestID
                        &&
                        r.CitizenID ==
                        citizenId
                );


            if (request == null)
            {
                return HttpNotFound();
            }


            // -----------------------------------------------------
            // Make sure request is still editable
            // -----------------------------------------------------

            if (!request.CanEdit())
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest,
                    "This request can no longer be edited."
                );
            }


            // -----------------------------------------------------
            // Update ONLY citizen-editable fields
            // -----------------------------------------------------

            request.Title =
                updatedRequest.Title;

            request.Description =
                updatedRequest.Description;

            request.LocationDescription =
                updatedRequest.LocationDescription;

            request.CategoryID =
                updatedRequest.CategoryID;


            // -----------------------------------------------------
            // Replace uploaded image
            // -----------------------------------------------------

            if (ImageFile != null &&
                ImageFile.ContentLength > 0)
            {
                string uploadFolder =
                    Server.MapPath("~/Uploads/");


                if (!System.IO.Directory.Exists(
                    uploadFolder))
                {
                    System.IO.Directory.CreateDirectory(
                        uploadFolder
                    );
                }


                string fileName =
                    System.IO.Path.GetFileName(
                        ImageFile.FileName
                    );


                string path =
                    System.IO.Path.Combine(
                        uploadFolder,
                        fileName
                    );


                ImageFile.SaveAs(path);


                request.ImagePath =
                    "~/Uploads/" + fileName;
            }


            // -----------------------------------------------------
            // Save changes
            // -----------------------------------------------------

            db.SaveChanges();


            return RedirectToAction("Index");
        }


        // =========================================================
        // GET: Requests/Delete/5
        // =========================================================

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest
                );
            }


            if (Session["CitizenID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }


            int citizenId =
                (int)Session["CitizenID"];


            // Only find requests belonging to this citizen
            Request request = db.Requests
                .FirstOrDefault(
                    r =>
                        r.RequestID == id &&
                        r.CitizenID == citizenId
                );


            if (request == null)
            {
                return HttpNotFound();
            }


            // -----------------------------------------------------
            // Only Pending requests can be cancelled
            // -----------------------------------------------------

            if (!request.CanEdit())
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest,
                    "This request can no longer be cancelled."
                );
            }


            return View(request);
        }


        // =========================================================
        // POST: Requests/Delete/5
        // =========================================================

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (Session["CitizenID"] == null)
            {
                return RedirectToAction("Index", "Requests");
            }


            int citizenId =
                (int)Session["CitizenID"];


            // Only find requests belonging to this citizen
            Request request = db.Requests
                .FirstOrDefault(
                    r =>
                        r.RequestID == id &&
                        r.CitizenID == citizenId
                );


            if (request == null)
            {
                return HttpNotFound();
            }


            // -----------------------------------------------------
            // Only Pending requests can be cancelled
            // -----------------------------------------------------

            if (!request.CanEdit())
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest,
                    "This request can no longer be cancelled."
                );
            }


            // -----------------------------------------------------
            // Delete / cancel request
            // -----------------------------------------------------

            db.Requests.Remove(request);

            db.SaveChanges();


            return RedirectToAction("Index");
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