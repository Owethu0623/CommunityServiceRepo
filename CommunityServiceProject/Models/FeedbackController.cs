using System.Linq;
using System.Web.Mvc;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly Community db = new Community();

        // GET: Feedback/Create
        public ActionResult Create(int? requestId)
        {
            if (Session["CitizenID"] == null)
            {
                return RedirectToAction("Login", "Login");
            }

            if (!requestId.HasValue)
            {
                return RedirectToAction("Index", "Requests");
            }

            int citizenId = (int)Session["CitizenID"];

            var request = db.Requests
                .Include("Category")
                .FirstOrDefault(r =>
                    r.RequestID == requestId.Value &&
                    r.CitizenID == citizenId);

            if (request == null)
            {
                return HttpNotFound();
            }

            // Feedback is only available after the request is completed.
            if (request.Status != RequestStatus.Completed)
            {
                TempData["FeedbackError"] =
                    "Feedback can only be provided for completed requests.";

                return RedirectToAction("Details", "Requests",
                    new { id = request.RequestID });
            }

            // Prevent duplicate feedback.
            bool alreadySubmitted = db.Feedbacks
                .Any(f => f.RequestID == request.RequestID);

            if (alreadySubmitted)
            {
                TempData["FeedbackError"] =
                    "Feedback has already been submitted for this request.";

                return RedirectToAction("Details", "Requests",
                    new { id = request.RequestID });
            }

            ViewBag.Request = request;

            var feedback = new Feedback
            {
                RequestID = request.RequestID,
                CitizenID = citizenId
            };

            return View(feedback);
        }

        // POST: Feedback/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Feedback feedback)
        {
            if (Session["CitizenID"] == null)
            {
                return RedirectToAction("Login", "Login");
            }

            int citizenId = (int)Session["CitizenID"];

            // Never trust CitizenID from the submitted form.
            feedback.CitizenID = citizenId;

            var request = db.Requests
                .Include("Category")
                .FirstOrDefault(r =>
                    r.RequestID == feedback.RequestID &&
                    r.CitizenID == citizenId);

            if (request == null)
            {
                return HttpNotFound();
            }

            // Request must be completed.
            if (request.Status != RequestStatus.Completed)
            {
                TempData["FeedbackError"] =
                    "Feedback can only be submitted for completed requests.";

                return RedirectToAction("Details", "Requests",
                    new { id = request.RequestID });
            }

            // Prevent duplicate feedback.
            bool alreadySubmitted = db.Feedbacks
                .Any(f => f.RequestID == request.RequestID);

            if (alreadySubmitted)
            {
                TempData["FeedbackError"] =
                    "Feedback has already been submitted for this request.";

                return RedirectToAction("Details", "Requests",
                    new { id = request.RequestID });
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Request = request;
                return View(feedback);
            }

            feedback.DateSubmitted = System.DateTime.Now;

            db.Feedbacks.Add(feedback);
            db.SaveChanges();

            return RedirectToAction("Submitted", new
            {
                id = feedback.FeedbackID
            });
        }

        // GET: Feedback/Submitted/5
        public ActionResult Submitted(int? id)
        {
            if (Session["CitizenID"] == null)
            {
                return RedirectToAction("Login", "Login");
            }

            if (!id.HasValue)
            {
                return HttpNotFound();
            }

            int citizenId = (int)Session["CitizenID"];

            var feedback = db.Feedbacks
                .Include("Request")
                .Include("Request.Category")
                .FirstOrDefault(f =>
                    f.FeedbackID == id.Value &&
                    f.CitizenID == citizenId);

            if (feedback == null)
            {
                return HttpNotFound();
            }

            return View(feedback);
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