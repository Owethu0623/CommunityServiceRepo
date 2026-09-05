using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CommunityServiceProject.Models
{
    public class CitizenProfileViewModel
    {
        [Required]
        [StringLength(50)]
        [RegularExpression(@"^[A-Za-z]+([ '-][A-Za-z]+)*$",
            ErrorMessage = "First name can contain letters, spaces, hyphens and apostrophes only.")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        [RegularExpression(@"^[A-Za-z]+([ '-][A-Za-z]+)*$",
            ErrorMessage = "Last name can contain letters, spaces, hyphens and apostrophes only.")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }

        [Required]
        [RegularExpression(@"^0\d{9}$",
            ErrorMessage = "Enter a valid 10-digit South African phone number.")]
        public string PhoneNumber { get; set; }

        [Required]
        [StringLength(200)]
        public string ResidentialAddress { get; set; }
    }
}