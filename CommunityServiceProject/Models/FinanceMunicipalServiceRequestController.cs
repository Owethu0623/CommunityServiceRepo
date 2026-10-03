
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using CommunityServiceProject.Models;
using CommunityServiceProject.Filters;

namespace CommunityServiceProject.Controllers
{
    public class FinanceMunicipalServiceRequestController : Controller
    {
        private readonly Community db = new Community();

        // =========================================================
        // US134 - VIEW MUNICIPAL SERVICE REQUESTS
        // Finance can view requests in ALL statuses.
        // =========================================================
        [RoleAuthorize("FinanceOfficer")]
        public ActionResult Index()
        {
            if (Session["FinanceOfficerID"] == null)
            {
                return new HttpUnauthorizedResult();
            }

            var requests = db.MunicipalServiceRequests
                .Include(x => x.Citizen)
                .Include(x => x.ServiceType)
                .OrderByDescending(x => x.DateSubmitted)
                .ToList();

            return View(requests);
        }

        // =========================================================
        // VIEW MUNICIPAL SERVICE REQUEST
        // Finance can inspect requests in ALL statuses.
        // =========================================================
        [RoleAuthorize("FinanceOfficer")]
        public ActionResult Details(int id)
        {
            if (Session["FinanceOfficerID"] == null)
            {
                return new HttpUnauthorizedResult();
            }

            var request = db.MunicipalServiceRequests
                .Include(x => x.Citizen)
                .Include(x => x.ServiceType)
                .Include(x => x.ReviewedByAdministrator)
                .FirstOrDefault(x =>
                    x.MunicipalServiceRequestID == id);

            if (request == null)
            {
                TempData["ErrorMessage"] =
                    "The municipal service request could not be found.";

                return RedirectToAction("Index");
            }

            return View(request);
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
