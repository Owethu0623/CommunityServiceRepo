using CommunityServiceProject.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Web.Mvc;

namespace CommunityServiceProject.Controllers
{
    public class AppealAccessController : Controller
    {
        private Community db = new Community();


        // =========================================================
        // GET: AppealAccess
        // =========================================================

        public ActionResult Index()
        {
            return View(new AppealAccessViewModel());
        }


        // =========================================================
        // POST: AppealAccess
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(
            AppealAccessViewModel model,
            string action)
        {
            // =====================================================
            // 1. SEND OTP
            // =====================================================

            if (action == "SendOTP")
            {
                if (string.IsNullOrWhiteSpace(model.EmailAddress))
                {
                    ModelState.AddModelError(
                        "EmailAddress",
                        "Please enter your registered email address."
                    );

                    return View(model);
                }

                if (!new System.ComponentModel.DataAnnotations
                    .EmailAddressAttribute()
                    .IsValid(model.EmailAddress))
                {
                    ModelState.AddModelError(
                        "EmailAddress",
                        "Please enter a valid email address."
                    );

                    return View(model);
                }

                var citizen = db.Citizens.FirstOrDefault(c =>
                    c.EmailAddress == model.EmailAddress
                );

                if (citizen == null)
                {
                    ModelState.AddModelError(
                        "EmailAddress",
                        "No account was found with this email address."
                    );

                    return View(model);
                }


                // =================================================
                // Check whether account can use Appeal Access
                // =================================================

                if (citizen.AccountStatus != AccountStatus.Suspended &&
                    citizen.AccountStatus != AccountStatus.Inactive)
                {
                    ModelState.AddModelError(
                        "",
                        "Your account is not currently eligible to submit an account appeal."
                    );

                    return View(model);
                }


                // =================================================
                // Check for an active restriction
                // =================================================

                var activeRestriction = db.AccountRestrictions
     .FirstOrDefault(r =>
         r.CitizenID == citizen.CitizenID &&
         r.IsActive &&
         r.DateStarted <= DateTime.Now &&
         (r.DateEnded == null ||
          r.DateEnded > DateTime.Now)
     );

                if (activeRestriction == null)
                {
                    ModelState.AddModelError(
                        "",
                        "No active account restriction was found for this account."
                    );

                    return View(model);
                }


                // =================================================
                // Check for an existing unresolved appeal
                // =================================================

                var existingAppeal = db.Appeals.FirstOrDefault(a =>
                    a.CitizenID == citizen.CitizenID &&
                    a.RestrictionID == activeRestriction.RestrictionID &&
                    (a.Status == AppealStatus.Pending ||
                     a.Status == AppealStatus.UnderReview
                     )
                );

                if (existingAppeal != null)
                {
                    ModelState.AddModelError(
                        "",
                        "You already have an active appeal for this account restriction."
                    );

                    return View(model);
                }


                // =================================================
                // Generate secure 6-digit OTP
                // =================================================

                string otp = GenerateOTP();


                // =================================================
                // Store Appeal Access OTP in Session
                // =================================================

                Session["AppealAccessOTP"] = otp;

                Session["AppealAccessEmail"] =
                    citizen.EmailAddress;

                Session["AppealAccessCitizenID"] =
                    citizen.CitizenID;

                Session["AppealAccessRestrictionID"] =
                    activeRestriction.RestrictionID;

                Session["AppealAccessOTPExpiry"] =
                    DateTime.Now.AddMinutes(5);

                Session["AppealAccessOTPAttempts"] = 0;


                // =================================================
                // Send OTP email
                // =================================================

                bool emailSent = SendOTPEmail(
                    citizen.EmailAddress,
                    otp
                );

                if (!emailSent)
                {
                    Session.Remove("AppealAccessOTP");
                    Session.Remove("AppealAccessEmail");
                    Session.Remove("AppealAccessCitizenID");
                    Session.Remove("AppealAccessRestrictionID");
                    Session.Remove("AppealAccessOTPExpiry");
                    Session.Remove("AppealAccessOTPAttempts");

                    ModelState.AddModelError(
                        "",
                        "We could not send the verification code. Please try again later."
                    );

                    return View(model);
                }


                ModelState.Clear();

                model.OTPsent = true;

                return View(model);
            }


            // =====================================================
            // 2. VERIFY OTP
            // =====================================================

            if (action == "VerifyOTP")
            {
                string storedOTP =
                    Session["AppealAccessOTP"] as string;

                string storedEmail =
                    Session["AppealAccessEmail"] as string;

                DateTime? expiry =
                    Session["AppealAccessOTPExpiry"] as DateTime?;

                int attempts =
                    Session["AppealAccessOTPAttempts"] != null
                    ? (int)Session["AppealAccessOTPAttempts"]
                    : 0;


                // =================================================
                // Verify session information exists
                // =================================================

                if (string.IsNullOrEmpty(storedOTP) ||
                    string.IsNullOrEmpty(storedEmail) ||
                    expiry == null)
                {
                    ModelState.AddModelError(
                        "",
                        "Your verification code has expired. Please request a new code."
                    );

                    return View(model);
                }


                // =================================================
                // Check expiry
                // =================================================

                if (DateTime.Now > expiry.Value)
                {
                    ClearAppealAccessSession();

                    ModelState.AddModelError(
                        "",
                        "Your verification code has expired. Please request a new code."
                    );

                    return View(model);
                }


                // =================================================
                // Check attempt limit
                // =================================================

                if (attempts >= 5)
                {
                    ClearAppealAccessSession();

                    ModelState.AddModelError(
                        "",
                        "Too many incorrect attempts. Please request a new verification code."
                    );

                    return View(model);
                }


                // =================================================
                // Empty OTP
                // =================================================

                if (string.IsNullOrWhiteSpace(model.OTP))
                {
                    ModelState.AddModelError(
                        "OTP",
                        "Please enter the verification code."
                    );

                    model.OTPsent = true;

                    return View(model);
                }


                // =================================================
                // Incorrect OTP
                // =================================================

                if (model.OTP != storedOTP)
                {
                    attempts++;

                    Session["AppealAccessOTPAttempts"] =
                        attempts;

                    ModelState.AddModelError(
                        "OTP",
                        "The verification code is incorrect."
                    );

                    model.OTPsent = true;

                    return View(model);
                }


                // =================================================
                // OTP verified successfully
                // =================================================

                Session["AppealAccessOTPVerified"] = true;

                // OTP can no longer be reused
                Session.Remove("AppealAccessOTP");

                ModelState.Clear();

                model.OTPsent = true;
                model.OTPVerified = true;


                // =================================================
                // Redirect to Appeal submission
                // =================================================

                return RedirectToAction(
                    "Create",
                    "Appeals"
                );
            }


            return View(model);
        }


        // =========================================================
        // GENERATE 6-DIGIT OTP
        // =========================================================

        private string GenerateOTP()
        {
            using (RandomNumberGenerator rng =
                   RandomNumberGenerator.Create())
            {
                byte[] bytes = new byte[4];

                rng.GetBytes(bytes);

                uint number = BitConverter.ToUInt32(
                    bytes,
                    0
                );

                return (number % 1000000)
                    .ToString("D6");
            }
        }


        // =========================================================
        // SEND OTP EMAIL
        // =========================================================

     
// =====================================================
// SEND OTP EMAIL
// =====================================================
private bool SendOTPEmail(
    string recipientEmail,
    string otp)
        {
            try
            {
                // -------------------------------------------------
                // Use the same Gmail account and App Password
                // configured for your working Forgot Password OTP.
                // -------------------------------------------------

                string senderEmail =
                    "hlongwaneowe@gmail.com";

                string senderPassword =
                    "likkmkdvptjaopfj";

                MailMessage mail =
                    new MailMessage();

                mail.From = new MailAddress(
                    senderEmail,
                    "Community Service Request System"
                );

                mail.To.Add(recipientEmail);

                mail.Subject =
                    "Account Appeal Verification Code";

                mail.Body =
                    "Hello,\n\n" +
                    "Your account appeal verification code is:\n\n" +
                    otp +
                    "\n\n" +
                    "This code will expire in 5 minutes." +
                    "\n\n" +
                    "If you did not request access to the account appeal process, " +
                    "please ignore this email." +
                    "\n\n" +
                    "Community Service Request System";

                mail.IsBodyHtml = false;

                SmtpClient smtp =
                    new SmtpClient(
                        "smtp.gmail.com",
                        587
                    );

                smtp.EnableSsl = true;

                smtp.Credentials =
                    new NetworkCredential(
                        senderEmail,
                        senderPassword
                    );

                smtp.Send(mail);

                return true;
            }
            catch
            {
                return false;
            }
        }


        // =========================================================
        // CLEAR APPEAL ACCESS SESSION
        // =========================================================

        private void ClearAppealAccessSession()
        {
            Session.Remove("AppealAccessOTP");
            Session.Remove("AppealAccessEmail");
            Session.Remove("AppealAccessCitizenID");
            Session.Remove("AppealAccessRestrictionID");
            Session.Remove("AppealAccessOTPExpiry");
            Session.Remove("AppealAccessOTPAttempts");
            Session.Remove("AppealAccessOTPVerified");
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

