using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.Controllers
{
    public class AdministratorRequestsController : Controller
    {
        private Community db = new Community();

        public ActionResult Index(
     string search,
     RequestStatus? status,
     int? categoryId,
     string sortOrder)
        {
            // Make sure an administrator is logged in
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            var requests = db.Requests
                .Include("Citizen")
                .Include("Category")
                .AsQueryable();

            // Search by request title
            if (!string.IsNullOrWhiteSpace(search))
            {
                requests = requests.Where(r =>
                    r.Title.Contains(search));
            }

            // Filter by status
            if (status.HasValue)
            {
                requests = requests.Where(r =>
                    r.Status == status.Value);
            }

            // Filter by category
            if (categoryId.HasValue)
            {
                requests = requests.Where(r =>
                    r.CategoryID == categoryId.Value);
            }

            // Sorting
            switch (sortOrder)
            {
                case "oldest":
                    requests = requests.OrderBy(r => r.DateSubmitted);
                    break;

                default:
                    requests = requests.OrderByDescending(r => r.DateSubmitted);
                    break;
            }

            // Send filter options to the view
            ViewBag.Categories = new SelectList(
                db.Categories.OrderBy(c => c.CategoryName),
                "CategoryID",
                "CategoryName",
                categoryId
            );

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.SortOrder = sortOrder;

            return View(requests.ToList());
        }


        // GET: AdministratorRequests/Details/5
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

            var request = db.Requests
                .Include("Citizen")
                .Include("Category")
                .FirstOrDefault(r => r.RequestID == id);

            if (request == null)
            {
                return HttpNotFound();
            }

            return View(request);
        }

        // POST: AdministratorRequests/StartReview
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StartReview(int id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            var request = db.Requests.Find(id);

            if (request == null)
            {
                return HttpNotFound();
            }

            if (request.Status == RequestStatus.Pending)
            {
                request.Status = RequestStatus.UnderReview;
                db.SaveChanges();
            }

            return RedirectToAction("Details", new { id = id });
        }

        // POST: AdministratorRequests/Approve
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Approve(int id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            var request = db.Requests.Find(id);

            if (request == null)
            {
                return HttpNotFound();
            }

            if (request.Status == RequestStatus.UnderReview)
            {
                request.Status = RequestStatus.Approved;
                request.AdministratorID = (int)Session["AdministratorID"];

                db.SaveChanges();
            }

            return RedirectToAction("Details", new { id = id });
        }

        // POST: AdministratorRequests/Reject
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Reject(int id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            var request = db.Requests.Find(id);

            if (request == null)
            {
                return HttpNotFound();
            }

            if (request.Status == RequestStatus.UnderReview)
            {
                request.Status = RequestStatus.Rejected;
                request.AdministratorID = (int)Session["AdministratorID"];

                db.SaveChanges();
            }

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