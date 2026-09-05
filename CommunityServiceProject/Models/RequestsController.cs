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
            if (Session["CitizenID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            int citizenId = (int)Session["CitizenID"];

            var requests = db.Requests
                .Include(r => r.Category)
                .Include(r => r.Citizen)
                .Include(r => r.Ward)
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
                .Include(r => r.Ward)
                .Include(r => r.Technician)
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
                .Include(r => r.Ward)
                .Include(r => r.Technician)

                // Assignment information
                .Include(r => r.TechnicianAssignments)
                .Include("TechnicianAssignments.Technician")

                // Maintenance work
                .Include(r => r.MaintenanceWorks)
                .Include("MaintenanceWorks.ProgressRecords")
                .Include("MaintenanceWorks.Evidence")
                .Include("MaintenanceWorks.Completions")
                .Include("MaintenanceWorks.Completions.VerifiedByAdministrator")

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

            ViewBag.WardID = new SelectList(
                db.Wards.OrderBy(w => w.WardNumber),
                "WardID",
                "WardName"
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
                "Title,Description,CategoryID,WardID,ProblemLocation")]
            Request request,
            string Latitude,
            string Longitude,
            HttpPostedFileBase ImageFile)
        {
            if (Session["CitizenID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }


            // =====================================================
            // REMOVE AUTOMATIC MVC VALIDATION FOR GPS FIELDS
            // =====================================================
            // Latitude and Longitude are received as strings above.
            // They are validated manually below and then converted
            // to double values before being saved.

            ModelState.Remove("Latitude");
            ModelState.Remove("Longitude");


            // =====================================================
            // VALIDATE LATITUDE
            // =====================================================

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


            // =====================================================
            // VALIDATE LONGITUDE
            // =====================================================

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


            // =====================================================
            // SAVE REQUEST
            // =====================================================

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


                // =================================================
                // DEFAULT PRIORITY
                // =================================================

                request.Priority =
                    Priority.Medium;


                // =================================================
                // HANDLE IMAGE UPLOAD
                // =================================================

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


                // =================================================
                // SAVE TO DATABASE
                // =================================================

                db.Requests.Add(request);

                db.SaveChanges();


                // =================================================
                // GENERATE REQUEST REFERENCE
                // =================================================

                // RequestID is now available after the first save.
                request.ReferenceNumber =
                    "REQ-" +
                    DateTime.Now.Year +
                    "-" +
                    request.RequestID.ToString("D6");


                // Save the generated reference number.
                db.SaveChanges();


                // =================================================
                // SHOW REQUEST DETAILS AFTER SUBMISSION
                // =================================================

                return RedirectToAction(
                    "Details",
                    new { id = request.RequestID }
                );
            }


            // =====================================================
            // VALIDATION FAILED
            // =====================================================

            ViewBag.CategoryID = new SelectList(
                db.Categories,
                "CategoryID",
                "CategoryName",
                request.CategoryID
            );


            ViewBag.WardID = new SelectList(
                db.Wards.OrderBy(w => w.WardNumber),
                "WardID",
                "WardName",
                request.WardID
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


            // Only Pending requests can be edited
            if (!request.CanEdit())
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest,
                    "This request can no longer be edited."
                );
            }


            ViewBag.CategoryID = new SelectList(
                db.Categories,
                "CategoryID",
                "CategoryName",
                request.CategoryID
            );


            ViewBag.WardID = new SelectList(
                db.Wards.OrderBy(w => w.WardNumber),
                "WardID",
                "WardName",
                request.WardID
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
                "RequestID,Title,Description,CategoryID,WardID,ProblemLocation")]
            Request updatedRequest,
            HttpPostedFileBase ImageFile)
        {
            if (Session["CitizenID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            int citizenId =
                (int)Session["CitizenID"];

            ModelState.Remove("Latitude");
            ModelState.Remove("Longitude");


            // =====================================================
            // VALIDATION
            // =====================================================

            if (!ModelState.IsValid)
            {
                ViewBag.CategoryID = new SelectList(
                    db.Categories,
                    "CategoryID",
                    "CategoryName",
                    updatedRequest.CategoryID
                );


                ViewBag.WardID = new SelectList(
                    db.Wards.OrderBy(w => w.WardNumber),
                    "WardID",
                    "WardName",
                    updatedRequest.WardID
                );


                return View(updatedRequest);
            }


            // =====================================================
            // FIND ORIGINAL REQUEST
            // =====================================================

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


            // =====================================================
            // CHECK IF STILL EDITABLE
            // =====================================================

            if (!request.CanEdit())
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest,
                    "This request can no longer be edited."
                );
            }


            // =====================================================
            // UPDATE CITIZEN-EDITABLE FIELDS
            // =====================================================

            request.Title =
                updatedRequest.Title;

            request.Description =
                updatedRequest.Description;

            request.CategoryID =
                updatedRequest.CategoryID;

            request.WardID =
                updatedRequest.WardID;

            request.ProblemLocation =
                updatedRequest.ProblemLocation;


            // =====================================================
            // REPLACE IMAGE
            // =====================================================

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


            // =====================================================
            // SAVE CHANGES
            // =====================================================

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


            // Only Pending requests can be cancelled
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
                return RedirectToAction("Index", "Login");
            }


            int citizenId =
                (int)Session["CitizenID"];


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


            // Only Pending requests can be cancelled
            if (!request.CanEdit())
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest,
                    "This request can no longer be cancelled."
                );
            }


            // Delete request
            db.Requests.Remove(request);

            db.SaveChanges();


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