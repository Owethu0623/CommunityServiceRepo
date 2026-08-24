using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CommunityServiceProject.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }

        public string OTP { get; set; }

        [DataType(DataType.Password)]
        [StringLength(
            100,
            MinimumLength = 8,
            ErrorMessage = "Password must be at least 8 characters long."
        )]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
            ErrorMessage = "Password must contain an uppercase letter, a lowercase letter, and a number."
        )]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare(
            "NewPassword",
            ErrorMessage = "Passwords do not match."
        )]
        public string ConfirmPassword { get; set; }

        public bool OTPsent { get; set; }

        public bool OTPVerified { get; set; }
    }
}