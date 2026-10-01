
using CommunityServiceProject.Models;
using CommunityServiceProject.ViewModels;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace CommunityServiceProject.Controllers
{
    public class AdministratorMunicipalServiceRequestsController : Controller
    {
        private readonly Community db = new Community();

        private bool IsAdministrator()
        {
            return Session["AdministratorID"] != null;
        }

        [HttpGet]
        public ActionResult Index(
            string searchTerm,
            string statusFilter,
            int? serviceTypeFilter)
        {
            if (!IsAdministrator())
                return RedirectToAction("Login", "Administrators");

            var query = db.MunicipalServiceRequests
                .Include(r => r.Citizen)
                .Include(r => r.ServiceType)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();

                query = query.Where(r =>
                    r.ReferenceNumber.Contains(searchTerm) ||
                    r.Title.Contains(searchTerm) ||
                    r.Citizen.FirstName.Contains(searchTerm) ||
                    r.Citizen.LastName.Contains(searchTerm) ||
                    r.Citizen.EmailAddress.Contains(searchTerm));
            }

            MunicipalServiceRequestStatus parsedStatus;

            if (!string.IsNullOrWhiteSpace(statusFilter) &&
                Enum.TryParse(statusFilter, true, out parsedStatus))
            {
                query = query.Where(r => r.Status == parsedStatus);
            }

            if (serviceTypeFilter.HasValue)
            {
                query = query.Where(r =>
                    r.ServiceTypeID == serviceTypeFilter.Value);
            }

            var requests = query
                .OrderByDescending(r => r.DateSubmitted)
                .ToList();

            var model = new AdministratorMunicipalServiceRequestsViewModel
            {
                SearchTerm = searchTerm,
                StatusFilter = statusFilter,
                ServiceTypeFilter = serviceTypeFilter,

                TotalRequests = db.MunicipalServiceRequests.Count(),

                SubmittedRequests = db.MunicipalServiceRequests.Count(
                    r => r.Status == MunicipalServiceRequestStatus.Submitted),

                UnderReviewRequests = db.MunicipalServiceRequests.Count(
                    r => r.Status == MunicipalServiceRequestStatus.UnderReview),

                ApprovedRequests = db.MunicipalServiceRequests.Count(
                    r => r.Status == MunicipalServiceRequestStatus.Approved),

                RejectedRequests = db.MunicipalServiceRequests.Count(
                    r => r.Status == MunicipalServiceRequestStatus.Rejected),

                Requests = requests
                    .Select(r => new AdministratorMunicipalServiceRequestListItemViewModel
                    {
                        MunicipalServiceRequestID = r.MunicipalServiceRequestID,
                        ReferenceNumber = r.ReferenceNumber,
                        CitizenName = r.Citizen.FirstName + " " + r.Citizen.LastName,
                        CitizenEmail = r.Citizen.EmailAddress,
                        ServiceCode = r.ServiceType.ServiceCode,
                        ServiceName = r.ServiceType.ServiceName,
                        Title = r.Title,
                        Status = r.Status,
                        DateSubmitted = r.DateSubmitted,
                        DateReviewed = r.DateReviewed,
                        DateApproved = r.DateApproved
                    })
                    .ToList(),

                ServiceTypes = db.ServiceTypes
                    .OrderBy(s => s.ServiceName)
                    .Select(s => new ServiceTypeFilterItemViewModel
                    {
                        ServiceTypeID = s.ServiceTypeID,
                        ServiceName = s.ServiceName
                    })
                    .ToList()
            };

            return View(model);
        }

        
[HttpGet]
public ActionResult Review(int id)
        {
            if (!IsAdministrator())
                return RedirectToAction("Login", "Administrators");

            var request = db.MunicipalServiceRequests
                .Include(r => r.Citizen)
                .Include(r => r.ServiceType)
                .Include(r => r.ReviewedByAdministrator)
                .FirstOrDefault(r => r.MunicipalServiceRequestID == id);

            if (request == null)
            {
                TempData["ErrorMessage"] =
                    "The selected municipal service request could not be found.";

                return RedirectToAction("Index");
            }

            /*
             * A submitted request enters the administrator review stage
             * when the administrator opens it for the first time.
             */
            if (request.Status == MunicipalServiceRequestStatus.Submitted)
            {
                var administratorId =
                    Convert.ToInt32(Session["AdministratorID"]);

                request.Status = MunicipalServiceRequestStatus.UnderReview;
                request.DateReviewed = DateTime.Now;
                request.ReviewedByAdministratorID = administratorId;

                db.SaveChanges();
            }

            var model = new AdministratorMunicipalServiceRequestReviewViewModel
            {
                MunicipalServiceRequestID = request.MunicipalServiceRequestID,

                ReferenceNumber = request.ReferenceNumber,

                CitizenID = request.CitizenID,
                CitizenName =
                    request.Citizen.FirstName + " " +
                    request.Citizen.LastName,
                CitizenEmail = request.Citizen.EmailAddress,
                CitizenPhoneNumber = request.Citizen.PhoneNumber,
                CitizenAddress = request.Citizen.ResidentialAddress,

                ServiceTypeID = request.ServiceTypeID,
                ServiceCode = request.ServiceType.ServiceCode,
                ServiceName = request.ServiceType.ServiceName,
                ServiceDescription = request.ServiceType.Description,

                Title = request.Title,
                Description = request.Description,
                AdditionalInformation = request.AdditionalInformation,

                Status = request.Status,

                DateSubmitted = request.DateSubmitted,
                DateReviewed = request.DateReviewed,
                DateApproved = request.DateApproved,
                DateCompleted = request.DateCompleted,
                RejectionReason = request.RejectionReason,
                ReviewedByAdministratorName =
                    request.ReviewedByAdministrator != null
                        ? request.ReviewedByAdministrator.FirstName + " " +
                          request.ReviewedByAdministrator.LastName
                        : null
            };

            return View(model);
        }

        
[HttpPost]
[ValidateAntiForgeryToken]
public ActionResult Approve(int id)
        {
            if (!IsAdministrator())
                return RedirectToAction("Login", "Administrators");

            var request = db.MunicipalServiceRequests
                .FirstOrDefault(r => r.MunicipalServiceRequestID == id);

            if (request == null)
            {
                TempData["ErrorMessage"] =
                    "The selected municipal service request could not be found.";

                return RedirectToAction("Index");
            }

            if (request.Status != MunicipalServiceRequestStatus.UnderReview)
            {
                TempData["ErrorMessage"] =
                    "This request can no longer be approved because it is not under review.";

                return RedirectToAction("Review", new { id = id });
            }

            var administratorId =
                Convert.ToInt32(Session["AdministratorID"]);

            request.Status = MunicipalServiceRequestStatus.Approved;
            request.DateApproved = DateTime.Now;

            if (!request.DateReviewed.HasValue)
                request.DateReviewed = DateTime.Now;

            request.ReviewedByAdministratorID = administratorId;

            var notificationService =
    new CommunityServiceProject.Services
        .MunicipalServiceRequestNotificationService(db);

            notificationService.Create(
                request,
                MunicipalServiceRequestNotificationType.RequestApproved,
                "Municipal Service Request Approved",
                "Your municipal service request " +
                request.ReferenceNumber +
                " has been approved.");

            db.SaveChanges();

            TempData["SuccessMessage"] =
                "Municipal service request " +
                request.ReferenceNumber +
                " has been approved successfully.";

            return RedirectToAction("Review", new { id = id });
        }

        

[HttpPost]
[ValidateAntiForgeryToken]
public ActionResult Reject(int id, string rejectionReason)
        {
            if (!IsAdministrator())
                return RedirectToAction("Login", "Administrators");

            var request = db.MunicipalServiceRequests
                .FirstOrDefault(r => r.MunicipalServiceRequestID == id);

            if (request == null)
            {
                TempData["ErrorMessage"] =
                    "The selected municipal service request could not be found.";

                return RedirectToAction("Index");
            }

            if (request.Status != MunicipalServiceRequestStatus.UnderReview)
            {
                TempData["ErrorMessage"] =
                    "This request can no longer be rejected because it is not under review.";

                return RedirectToAction("Review", new { id = id });
            }

            if (string.IsNullOrWhiteSpace(rejectionReason))
            {
                TempData["ErrorMessage"] =
                    "A rejection reason is required.";

                return RedirectToAction("Review", new { id = id });
            }

            rejectionReason = rejectionReason.Trim();

            if (rejectionReason.Length > 2000)
            {
                TempData["ErrorMessage"] =
                    "The rejection reason cannot exceed 2000 characters.";

                return RedirectToAction("Review", new { id = id });
            }

            var administratorId =
                Convert.ToInt32(Session["AdministratorID"]);

            request.Status = MunicipalServiceRequestStatus.Rejected;

            request.DateReviewed =
                request.DateReviewed ?? DateTime.Now;

            request.ReviewedByAdministratorID =
                administratorId;

            request.RejectionReason =
                rejectionReason;

            var notificationService =
    new CommunityServiceProject.Services
        .MunicipalServiceRequestNotificationService(db);

            notificationService.Create(
                request,
                MunicipalServiceRequestNotificationType.RequestRejected,
                "Municipal Service Request Rejected",
                "Your municipal service request " +
                request.ReferenceNumber +
                " has been rejected. Reason: " +
                request.RejectionReason);

            db.SaveChanges();

            /*
             * Notification will be handled through the existing
             * citizen notification workflow.
             */

            TempData["SuccessMessage"] =
                "Municipal service request " +
                request.ReferenceNumber +
                " has been rejected successfully.";

            return RedirectToAction("Review", new { id = id });
        }





        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}

