using System;
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
                // Citizen does not exist / incorrect credentials
                // =================================================

                if (citizen == null)
                {
                    ModelState.AddModelError(
                        "",
                        "Invalid email or password."
                    );

                    return View(model);
                }


                // =================================================
                // CHECK LATEST APPROVED APPEAL
                // =================================================

                var latestAppeal = db.Appeals
                    .Where(a =>
                        a.CitizenID == citizen.CitizenID
                    )
                    .OrderByDescending(a => a.DateSubmitted)
                    .FirstOrDefault();


                // =================================================
                // ACCOUNT ACTIVE + APPROVED APPEAL
                // =================================================

                if (citizen.AccountStatus == AccountStatus.Active &&
                    latestAppeal != null &&
                    latestAppeal.Status ==
                        CommunityServiceProject.Models.AppealStatus.Approved &&
                    !latestAppeal.ApprovalAcknowledged)
                {
                    return RedirectToAction(
                        "AppealApproved",
                        new { id = latestAppeal.AppealID }
                    );
                }


                // =================================================
                // ACCOUNT SUSPENDED / INACTIVE
                // =================================================

                if (citizen.AccountStatus == AccountStatus.Suspended ||
                    citizen.AccountStatus == AccountStatus.Inactive)
                {
                    // Remember the citizen who successfully supplied
                    // valid login credentials but cannot enter the system
                    Session["RestrictedCitizenID"] = citizen.CitizenID;

                    ModelState.AddModelError(
                        "",
                        citizen.AccountStatus == AccountStatus.Suspended
                            ? "Your account has been suspended."
                            : "Your account is inactive."
                    );


                    // ---------------------------------------------
                    // Find the current active account restriction
                    // ---------------------------------------------

                    var activeRestriction = db.AccountRestrictions
                        .FirstOrDefault(r =>
                            r.CitizenID == citizen.CitizenID &&
                            r.IsActive &&
                            r.DateStarted <= DateTime.Now &&
                            (r.DateEnded == null ||
                             r.DateEnded > DateTime.Now)
                        );


                    // ---------------------------------------------
                    // Find appeal linked to the current restriction
                    // ---------------------------------------------

                    var currentAppeal = activeRestriction == null
                        ? null
                        : db.Appeals
                            .Where(a =>
                                a.CitizenID == citizen.CitizenID &&
                                a.RestrictionID ==
                                    activeRestriction.RestrictionID
                            )
                            .OrderByDescending(a =>
                                a.Status ==
                                    CommunityServiceProject.Models.AppealStatus.Pending ||
                                a.Status ==
                                    CommunityServiceProject.Models.AppealStatus.UnderReview
                                    ? 1
                                    : 0
                            )
                            .ThenByDescending(a =>
                                a.DateSubmitted
                            )
                            .FirstOrDefault();


                    ViewBag.ShowAppealButton = true;


                    // =================================================
                    // NO APPEAL
                    // =================================================

                    if (currentAppeal == null)
                    {
                        ViewBag.AppealButtonText =
                            "Make an Appeal";

                        ViewBag.AppealButtonAction =
                            "MakeAppeal";

                        ViewBag.AccountStatusMessage =
                            citizen.AccountStatus ==
                            AccountStatus.Suspended

                                ? "Your account has been suspended. You can submit an account appeal to request a review."

                                : "Your account is inactive. You can submit an account appeal to request a review.";

                        return View(model);
                    }


                    // =================================================
                    // PENDING / UNDER REVIEW
                    // =================================================

                    if (currentAppeal.Status ==
                            CommunityServiceProject.Models.AppealStatus.Pending ||
                        currentAppeal.Status ==
                            CommunityServiceProject.Models.AppealStatus.UnderReview)
                    {
                        ViewBag.AppealButtonText =
                            "View Appeal Status";

                        ViewBag.AppealButtonAction =
                            "ViewStatus";

                        ViewBag.AppealID =
                            currentAppeal.AppealID;

                        ViewBag.AccountStatusMessage =
                            "You have already submitted an appeal for this account restriction. You can view your current appeal status.";

                        return View(model);
                    }


                    // =================================================
                    // REJECTED
                    // =================================================

                    if (currentAppeal.Status ==
                        CommunityServiceProject.Models.AppealStatus.Rejected)
                    {
                        ViewBag.AppealButtonText =
                            "View Appeal Status";

                        ViewBag.AppealButtonAction =
                            "ViewStatus";

                        ViewBag.AppealID =
                            currentAppeal.AppealID;

                        ViewBag.AccountStatusMessage =
                            "Your previous appeal was rejected. You can view the administrator's decision and submit a new appeal.";

                        return View(model);
                    }


                    return View(model);
                }


                // =================================================
                // LOGIN SUCCESSFUL
                // =================================================

                Session["CitizenID"] =
                    citizen.CitizenID;

                Session["CitizenName"] =
                    citizen.FirstName;

                Session["CitizenEmail"] =
                    citizen.EmailAddress;


                return RedirectToAction(
                    "Index",
                    "CitizenDashboard"
                );
            }


            return View(model);
        }


        // =========================================================
        // APPEAL APPROVED
        // =========================================================

        public ActionResult AppealApproved(int id)
        {
            var appeal = db.Appeals
                .FirstOrDefault(a =>
                    a.AppealID == id &&
                    a.Status == CommunityServiceProject.Models.AppealStatus.Approved
                );

            if (appeal == null)
            {
                return HttpNotFound();
            }


            // Make sure the appeal belongs to the citizen
            var citizen = db.Citizens
                .FirstOrDefault(c =>
                    c.CitizenID == appeal.CitizenID
                );

            if (citizen == null)
            {
                return HttpNotFound();
            }


            // Remember permanently that the citizen
            // has seen this approval message.
            appeal.ApprovalAcknowledged = true;

            db.SaveChanges();


            appeal.Citizen = citizen;

            appeal.Administrator = appeal.AdministratorID.HasValue
                ? db.Administrators.FirstOrDefault(a =>
                    a.AdministratorID ==
                    appeal.AdministratorID.Value)
                : null;


            return View(appeal);
        }

        // =========================================================
        // CITIZEN APPEAL STATUS
        // =========================================================

        public ActionResult AppealStatus(int id)
        {
            // The citizen must have successfully entered
            // valid credentials for the restricted account.
            if (Session["RestrictedCitizenID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            int citizenID = (int)Session["RestrictedCitizenID"];

            // Find the appeal and make sure it belongs
            // to the citizen who just attempted to log in.
            var appeal = db.Appeals
                .FirstOrDefault(a =>
                    a.AppealID == id &&
                    a.CitizenID == citizenID);

            if (appeal == null)
            {
                return HttpNotFound();
            }

            // Load citizen information
            appeal.Citizen = db.Citizens
                .FirstOrDefault(c =>
                    c.CitizenID == appeal.CitizenID);

            // Load restriction information
            appeal.Restriction = db.AccountRestrictions
                .FirstOrDefault(r =>
                    r.RestrictionID == appeal.RestrictionID);

            // Load administrator information if the appeal
            // has already been reviewed.
            if (appeal.AdministratorID.HasValue)
            {
                appeal.Administrator = db.Administrators
                    .FirstOrDefault(a =>
                        a.AdministratorID ==
                        appeal.AdministratorID.Value);
            }

            return View(appeal);
        }


        // =========================================================
        // GET: Logout
        // =========================================================

        public ActionResult Logout()
        {
            Session.Remove("CitizenID");
            Session.Remove("CitizenName");
            Session.Remove("CitizenEmail");

            Session.Remove("RestrictedCitizenID");

            return RedirectToAction(
                "Index",
                "Home"
            );
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