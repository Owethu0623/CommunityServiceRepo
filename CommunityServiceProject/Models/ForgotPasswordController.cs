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
    public class ForgotPasswordController : Controller
    {
        private Community db = new Community();

        // GET: ForgotPassword
        public ActionResult Index()
        {
            return View(new ForgotPasswordViewModel());
        }


        // POST: ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(
            ForgotPasswordViewModel model,
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

                var citizen = db.Citizens.FirstOrDefault(c =>
                    c.EmailAddress == model.EmailAddress);

                if (citizen == null)
                {
                    ModelState.AddModelError(
                        "EmailAddress",
                        "No account was found with this email address."
                    );

                    return View(model);
                }

                // Generate a secure 6-digit OTP
                string otp = GenerateOTP();

                // Store OTP information in Session
                Session["PasswordResetOTP"] = otp;
                Session["PasswordResetEmail"] =
                    citizen.EmailAddress;

                Session["PasswordResetOTPExpiry"] =
                    DateTime.Now.AddMinutes(5);

                Session["PasswordResetOTPAttempts"] = 0;

                // Send OTP email
                bool emailSent = SendOTPEmail(
                    citizen.EmailAddress,
                    otp
                );

                if (!emailSent)
                {
                    Session.Remove("PasswordResetOTP");
                    Session.Remove("PasswordResetEmail");
                    Session.Remove("PasswordResetOTPExpiry");
                    Session.Remove("PasswordResetOTPAttempts");

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
                    Session["PasswordResetOTP"] as string;

                string storedEmail =
                    Session["PasswordResetEmail"] as string;

                DateTime? expiry =
                    Session["PasswordResetOTPExpiry"]
                    as DateTime?;

                int attempts =
                    Session["PasswordResetOTPAttempts"] != null
                    ? (int)Session["PasswordResetOTPAttempts"]
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
                    Session.Remove("PasswordResetOTP");
                    Session.Remove("PasswordResetEmail");
                    Session.Remove("PasswordResetOTPExpiry");
                    Session.Remove("PasswordResetOTPAttempts");

                    ModelState.AddModelError(
                        "",
                        "Your verification code has expired. Please request a new code."
                    );

                    return View(model);
                }

                if (attempts >= 5)
                {
                    Session.Remove("PasswordResetOTP");
                    Session.Remove("PasswordResetEmail");
                    Session.Remove("PasswordResetOTPExpiry");
                    Session.Remove("PasswordResetOTPAttempts");

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

                    Session["PasswordResetOTPAttempts"] =
                        attempts;

                    ModelState.AddModelError(
                        "OTP",
                        "The verification code is incorrect."
                    );

                    model.OTPsent = true;

                    return View(model);
                }

                // OTP is correct
                Session["PasswordResetOTPVerified"] = true;

                // OTP has now been used and must not be reusable
                Session.Remove("PasswordResetOTP");

                ModelState.Clear();

                model.OTPsent = true;
                model.OTPVerified = true;

                return View(model);
            }


            // =====================================================
            // 3. RESET PASSWORD
            // =====================================================
            if (action == "ResetPassword")
            {
                bool otpVerified =
                    Session["PasswordResetOTPVerified"] != null &&
                    (bool)Session["PasswordResetOTPVerified"];

                string verifiedEmail =
                    Session["PasswordResetEmail"] as string;

                DateTime? expiry =
                    Session["PasswordResetOTPExpiry"] as DateTime?;

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
                    Session.Remove("PasswordResetOTP");
                    Session.Remove("PasswordResetEmail");
                    Session.Remove("PasswordResetOTPExpiry");
                    Session.Remove("PasswordResetOTPAttempts");
                    Session.Remove("PasswordResetOTPVerified");

                    ModelState.AddModelError(
                        "",
                        "Your verification session has expired. Please request a new verification code."
                    );

                    return View(model);
                }

                var citizen = db.Citizens.FirstOrDefault(c =>
                    c.EmailAddress == verifiedEmail);

                if (citizen == null)
                {
                    ModelState.AddModelError(
                        "",
                        "The account could not be found."
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

                citizen.Password = model.NewPassword;
                citizen.ConfirmPassword = model.ConfirmPassword;

                db.SaveChanges();

                // Clear password-reset session information
                Session.Remove("PasswordResetOTP");
                Session.Remove("PasswordResetEmail");
                Session.Remove("PasswordResetOTPExpiry");
                Session.Remove("PasswordResetOTPAttempts");
                Session.Remove("PasswordResetOTPVerified");

                TempData["Message"] =
                    "Your password has been successfully reset.";

                return RedirectToAction(
                    "Index",
                    "Login"
                );
            }

            return View(model);
        }


        // =====================================================
        // GENERATE 6-DIGIT OTP
        // =====================================================
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
                // IMPORTANT:
                // Replace these with your email account details.
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
                    "Password Reset Verification Code";

                mail.Body =
                    "Hello,\n\n" +
                    "Your password reset verification code is:\n\n" +
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