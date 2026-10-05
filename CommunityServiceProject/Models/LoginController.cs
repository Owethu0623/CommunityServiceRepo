using System;
using System.Linq;
using System.Web.Mvc;
using CommunityServiceProject.Models;
using CommunityServiceProject.Helpers;

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

                // =========================================================
                // CHECK FINANCE OFFICER LOGIN
                // =========================================================

                // =========================================================
                // Finance officer: find by email and verify password (supports hashed or legacy plaintext)
                // =========================================================
                var financeOfficer = db.FinanceOfficers.FirstOrDefault(f =>
                    f.EmailAddress == model.EmailAddress
                );

                if (financeOfficer != null &&
                    (PasswordHelper.VerifyHashedPassword(financeOfficer.Password, model.Password) || financeOfficer.Password == model.Password))
                {
                    // If the stored password is legacy plaintext, migrate it to a hashed value
                    if (!PasswordHelper.VerifyHashedPassword(financeOfficer.Password, model.Password) && financeOfficer.Password == model.Password)
                    {
                        financeOfficer.Password = PasswordHelper.HashPassword(model.Password);
                        db.SaveChanges();
                    }
                    // ---------------------------------------------
                    // Finance Officer account status
                    // ---------------------------------------------

                    if (financeOfficer.AccountStatus != AccountStatus.Active)
                    {
                        ModelState.AddModelError(
                            "",
                            financeOfficer.AccountStatus == AccountStatus.Suspended
                                ? "Your Finance Officer account has been suspended."
                                : "Your Finance Officer account is inactive."
                        );

                        return View(model);
                    }

                    // ---------------------------------------------
                    // Finance Officer session
                    // ---------------------------------------------

                    Session["FinanceOfficerID"] =
                        financeOfficer.FinanceOfficerID;

                    Session["FinanceOfficerName"] =
                        financeOfficer.FirstName;

                    Session["FinanceOfficerEmail"] =
                        financeOfficer.EmailAddress;

                    Session["UserRole"] =
                        "FinanceOfficer";

                    // ---------------------------------------------
                    // Finance Dashboard
                    // ---------------------------------------------

                    return RedirectToAction(
                        "Index",
                        "FinanceDashboard"
                    );
                }

                // =========================================================
                // CHECK HR OFFICER LOGIN
                // =========================================================

                var hrOfficer = db.HROfficers.FirstOrDefault(h =>
                    h.EmailAddress == model.EmailAddress
                );

                if (hrOfficer != null &&
                    (PasswordHelper.VerifyHashedPassword(hrOfficer.Password, model.Password) || hrOfficer.Password == model.Password))
                {
                    if (hrOfficer.AccountStatus != AccountStatus.Active)
                    {
                        ModelState.AddModelError(
                            "",
                            hrOfficer.AccountStatus == AccountStatus.Suspended
                                ? "Your HR Officer account has been suspended."
                                : "Your HR Officer account is inactive."
                        );

                        return View(model);
                    }

                    Session["HROfficerID"] = hrOfficer.HROfficerID;
                    Session["HROfficerName"] = hrOfficer.FirstName;
                    Session["HROfficerEmail"] = hrOfficer.EmailAddress;
                    Session["UserRole"] = "HROfficer";

                    // Clear other role sessions to avoid role leakage
                    Session.Remove("AdministratorID");
                    Session.Remove("FinanceOfficerID");
                    Session.Remove("CitizenID");
                    Session.Remove("TechnicianID");

                    // Redirect HR officers to the TechnicianOpportunity management page
                    return RedirectToAction("Manage", "TechnicianOpportunity");
                }

                // =========================================================
                // CHECK ADMINISTRATOR LOGIN
                // =========================================================

                var administrator = db.Administrators.FirstOrDefault(a =>
                    a.EmailAddress == model.EmailAddress
                );

                if (administrator != null &&
                    (PasswordHelper.VerifyHashedPassword(administrator.Password, model.Password) || administrator.Password == model.Password))
                {
                    if (administrator.AccountStatus != AccountStatus.Active)
                    {
                        ModelState.AddModelError(
                            "",
                            administrator.AccountStatus == AccountStatus.Suspended
                                ? "Your Administrator account has been suspended."
                                : "Your Administrator account is inactive."
                        );

                        return View(model);
                    }

                    Session["AdministratorID"] = administrator.AdministratorID;
                    Session["AdministratorName"] = administrator.FirstName;
                    Session["UserRole"] = "Administrator";

                    // Clear other role sessions to avoid role leakage
                    Session.Remove("FinanceOfficerID");
                    Session.Remove("HROfficerID");
                    Session.Remove("CitizenID");
                    Session.Remove("TechnicianID");

                    return RedirectToAction("Index", "AdministratorDashboard");
                }

                // =========================================================
                // CHECK TECHNICIAN LOGIN
                // =========================================================

                var technician = db.Technicians.FirstOrDefault(t =>
                    t.EmailAddress == model.EmailAddress
                );

                if (technician != null &&
                    (PasswordHelper.VerifyHashedPassword(technician.Password, model.Password) || technician.Password == model.Password))
                {
                    if (technician.AccountStatus != AccountStatus.Active)
                    {
                        ModelState.AddModelError(
                            "",
                            technician.AccountStatus == AccountStatus.Suspended
                                ? "Your Technician account has been suspended."
                                : "Your Technician account is inactive."
                        );

                        return View(model);
                    }

                    Session["TechnicianID"] = technician.TechnicianID;
                    Session["TechnicianName"] = technician.FirstName + " " + technician.LastName;
                    Session["UserRole"] = "Technician";

                    // Force first-time password change if required
                    if (technician.MustChangePassword)
                    {
                        return RedirectToAction("ChangePassword", "Technicians");
                    }

                    // Ensure any other role sessions are cleared to avoid role leakage
                    Session.Remove("AdministratorID");
                    Session.Remove("FinanceOfficerID");
                    Session.Remove("HROfficerID");
                    Session.Remove("CitizenID");

                    return RedirectToAction("Dashboard", "Technicians");
                }


                // =========================================================
                // EXISTING CITIZEN LOGIN
                // =========================================================

               
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
                // CHECK ACTIVE 30-DAY ACCOUNT RESTRICTION
                // =================================================

                var loginRestriction = db.AccountRestrictions
                    .FirstOrDefault(r =>
                        r.CitizenID == citizen.CitizenID &&
                        r.IsActive &&
                        r.DateStarted <= DateTime.Now &&
                        r.DateEnded.HasValue &&
                        r.DateEnded.Value > DateTime.Now
                    );

                if (loginRestriction != null)
                {
                    // Remember the restricted citizen so that
                    // restriction/appeal functionality can be accessed.
                    Session["RestrictedCitizenID"] = citizen.CitizenID;

                    ViewBag.IsRestricted = true;

                    ViewBag.RestrictionStartDate =
                        loginRestriction.DateStarted;

                    ViewBag.RestrictionEndDate =
                        loginRestriction.DateEnded.Value;

                    ViewBag.RestrictionReason =
                        loginRestriction.Reason;

                    ViewBag.AccountStatusMessage =
                        "Your account is currently under a 30-day restriction. You cannot access the Municipal Service Platform while this restriction is active.";

                    ModelState.AddModelError(
                        "",
                        "Your account is restricted until "
                        + loginRestriction.DateEnded.Value.ToString("dd MMMM yyyy")
                        + "."
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

                                ? "Your account has been suspended due to non-compliance with system rules. You can submit an account appeal to request a review."

                                : "Your account is inactive non-compliance with system rules. You can submit an account appeal to request a review.";

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
            // Clear all role sessions to avoid leaking roles between logins
            Session.Remove("CitizenID");
            Session.Remove("CitizenName");
            Session.Remove("CitizenEmail");

            Session.Remove("RestrictedCitizenID");

            Session.Remove("AdministratorID");
            Session.Remove("AdministratorName");

            Session.Remove("TechnicianID");
            Session.Remove("TechnicianName");

            Session.Remove("FinanceOfficerID");
            Session.Remove("FinanceOfficerName");

            Session.Remove("HROfficerID");
            Session.Remove("HROfficerName");
            Session.Remove("UserRole");

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