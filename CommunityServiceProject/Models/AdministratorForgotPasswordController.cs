using CommunityServiceProject.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Web.Mvc;

namespace CommunityServiceProject.Controllers
{
    public class AdministratorForgotPasswordController : Controller
    {
        private Community db = new Community();


        // =====================================================
        // GET: AdministratorForgotPassword
        // =====================================================

        public ActionResult Index()
        {
            return View(new ForgotPasswordViewModel());
        }


        // =====================================================
        // POST: AdministratorForgotPassword
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(
            ForgotPasswordViewModel model,
            string action)
        {

            // =================================================
            // 1. SEND OTP
            // =================================================

            if (action == "SendOTP")
            {
                if (string.IsNullOrWhiteSpace(model.EmailAddress))
                {
                    ModelState.AddModelError(
                        "EmailAddress",
                        "Please enter your email address."
                    );

                    return View(model);
                }


                if (!new EmailAddressAttribute()
                    .IsValid(model.EmailAddress))
                {
                    ModelState.AddModelError(
                        "EmailAddress",
                        "Please enter a valid email address."
                    );

                    return View(model);
                }


                var administrator =
                    db.Administrators.FirstOrDefault(a =>
                        a.EmailAddress == model.EmailAddress &&
                        a.AccountStatus == AccountStatus.Active
                    );


                if (administrator == null)
                {
                    ModelState.AddModelError(
                        "EmailAddress",
                        "No active administrator account was found with this email address."
                    );

                    return View(model);
                }


                // Generate OTP

                string otp = GenerateOTP();


                // Store reset information

                Session["AdministratorPasswordResetOTP"] = otp;

                Session["AdministratorPasswordResetEmail"] =
                    administrator.EmailAddress;

                Session["AdministratorPasswordResetOTPExpiry"] =
                    DateTime.Now.AddMinutes(5);

                Session["AdministratorPasswordResetOTPAttempts"] = 0;

                Session["AdministratorPasswordResetOTPVerified"] =
                    false;


                // Send email

                bool emailSent =
                    SendOTPEmail(
                        administrator.EmailAddress,
                        otp
                    );


                if (!emailSent)
                {
                    ClearPasswordResetSession();

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


            // =================================================
            // 2. VERIFY OTP
            // =================================================

            if (action == "VerifyOTP")
            {
                string storedOTP =
                    Session["AdministratorPasswordResetOTP"]
                    as string;

                string storedEmail =
                    Session["AdministratorPasswordResetEmail"]
                    as string;

                DateTime? expiry =
                    Session["AdministratorPasswordResetOTPExpiry"]
                    as DateTime?;


                int attempts =
                    Session["AdministratorPasswordResetOTPAttempts"] != null
                    ? (int)Session["AdministratorPasswordResetOTPAttempts"]
                    : 0;


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


                if (DateTime.Now > expiry.Value)
                {
                    ClearPasswordResetSession();

                    ModelState.AddModelError(
                        "",
                        "Your verification code has expired. Please request a new code."
                    );

                    return View(model);
                }


                if (attempts >= 5)
                {
                    ClearPasswordResetSession();

                    ModelState.AddModelError(
                        "",
                        "Too many incorrect attempts. Please request a new verification code."
                    );

                    return View(model);
                }


                if (string.IsNullOrWhiteSpace(model.OTP))
                {
                    ModelState.AddModelError(
                        "OTP",
                        "Please enter the verification code."
                    );

                    model.OTPsent = true;

                    return View(model);
                }


                if (model.OTP != storedOTP)
                {
                    attempts++;

                    Session["AdministratorPasswordResetOTPAttempts"] =
                        attempts;

                    ModelState.AddModelError(
                        "OTP",
                        "The verification code is incorrect."
                    );

                    model.OTPsent = true;

                    return View(model);
                }


                // OTP correct

                Session["AdministratorPasswordResetOTPVerified"] =
                    true;


                // OTP cannot be reused

                Session.Remove(
                    "AdministratorPasswordResetOTP"
                );


                ModelState.Clear();

                model.OTPsent = true;
                model.OTPVerified = true;

                return View(model);
            }


            // =================================================
            // 3. RESET PASSWORD
            // =================================================

            if (action == "ResetPassword")
            {
                bool otpVerified =
                    Session["AdministratorPasswordResetOTPVerified"] != null &&
                    (bool)Session["AdministratorPasswordResetOTPVerified"];


                string verifiedEmail =
                    Session["AdministratorPasswordResetEmail"]
                    as string;


                DateTime? expiry =
                    Session["AdministratorPasswordResetOTPExpiry"]
                    as DateTime?;


                if (!otpVerified ||
                    string.IsNullOrEmpty(verifiedEmail))
                {
                    ModelState.AddModelError(
                        "",
                        "Please verify your email using the verification code first."
                    );

                    return View(model);
                }


                if (expiry == null ||
                    DateTime.Now > expiry.Value)
                {
                    ClearPasswordResetSession();

                    ModelState.AddModelError(
                        "",
                        "Your verification session has expired. Please request a new code."
                    );

                    return View(model);
                }


                var administrator =
                    db.Administrators.FirstOrDefault(a =>
                        a.EmailAddress == verifiedEmail &&
                        a.AccountStatus == AccountStatus.Active
                    );


                if (administrator == null)
                {
                    ModelState.AddModelError(
                        "",
                        "The administrator account could not be found."
                    );

                    return View(model);
                }


                if (string.IsNullOrEmpty(model.NewPassword))
                {
                    ModelState.AddModelError(
                        "NewPassword",
                        "Please enter a new password."
                    );

                    model.OTPsent = true;
                    model.OTPVerified = true;

                    return View(model);
                }


                if (model.NewPassword != model.ConfirmPassword)
                {
                    ModelState.AddModelError(
                        "ConfirmPassword",
                        "Passwords do not match."
                    );

                    model.OTPsent = true;
                    model.OTPVerified = true;

                    return View(model);
                }


                if (!ModelState.IsValid)
                {
                    model.OTPsent = true;
                    model.OTPVerified = true;

                    return View(model);
                }


                // Update administrator password

                administrator.Password =
                    model.NewPassword;


                db.SaveChanges();


                // Clear reset session

                ClearPasswordResetSession();


                TempData["Message"] =
                    "Your administrator password has been successfully reset.";


                return RedirectToAction(
                    "Login",
                    "Administrators"
                );
            }


            return View(model);
        }


        // =====================================================
        // GENERATE OTP
        // =====================================================

        private string GenerateOTP()
        {
            using (
                RandomNumberGenerator rng =
                RandomNumberGenerator.Create()
            )
            {
                byte[] bytes = new byte[4];

                rng.GetBytes(bytes);

                uint number =
                    BitConverter.ToUInt32(bytes, 0);

                return (number % 1000000)
                    .ToString("D6");
            }
        }


        // =====================================================
        // SEND OTP EMAIL
        // =====================================================

        private bool SendOTPEmail(
            string recipientEmail,
            string otp)
        {
            try
            {
                string senderEmail =
                    "hlongwaneowe@gmail.com";


                // IMPORTANT:
                // Put your NEW Gmail App Password here locally.

                string senderPassword =
                   "likkmkdvptjaopfj";


                MailMessage mail =
                    new MailMessage();


                mail.From =
                    new MailAddress(
                        senderEmail,
                        "Community Service Request System"
                    );


                mail.To.Add(recipientEmail);


                mail.Subject =
                    "Administrator Password Reset Verification Code";


                mail.Body =
                    "Hello,\n\n" +
                    "Your administrator password reset " +
                    "verification code is:\n\n" +
                    otp +
                    "\n\n" +
                    "This code will expire in 5 minutes." +
                    "\n\n" +
                    "If you did not request a password reset, " +
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


        // =====================================================
        // CLEAR RESET SESSION
        // =====================================================

        private void ClearPasswordResetSession()
        {
            Session.Remove(
                "AdministratorPasswordResetOTP"
            );

            Session.Remove(
                "AdministratorPasswordResetEmail"
            );

            Session.Remove(
                "AdministratorPasswordResetOTPExpiry"
            );

            Session.Remove(
                "AdministratorPasswordResetOTPAttempts"
            );

            Session.Remove(
                "AdministratorPasswordResetOTPVerified"
            );
        }


        // =====================================================
        // DISPOSE
        // =====================================================

        protected override void Dispose(
            bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}