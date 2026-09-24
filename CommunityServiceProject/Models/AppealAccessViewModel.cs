using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CommunityServiceProject.Models
{
    public class AppealAccessViewModel
    {
        [Required(ErrorMessage = "Please enter your registered email address.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string EmailAddress { get; set; }

        public string OTP { get; set; }

        public bool OTPsent { get; set; }

        public bool OTPVerified { get; set; }
    }
}

